using System;
using System.Collections;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using ZXing;

public class QrCodeScanService : MonoBehaviour
{
    enum QrScanBackend
    {
        Auto = 0,
        ArFoundation = 1,
        WebCam = 2
    }

    [Header("Kamera")]
    [SerializeField] QrScanBackend scanBackend = QrScanBackend.Auto;
    [SerializeField] ARCameraManager arCameraManager = null;
    [SerializeField] bool startScanningOnEnable = false;
    [SerializeField] int preferredCameraWidth = 1280;
    [SerializeField] int preferredCameraHeight = 720;
    [SerializeField, Range(1, 30)] int targetScanFps = 8;
    [SerializeField, Min(0.1f)] float rescanCooldownSeconds = 1.5f;

    [Header("Önizleme")]
    [SerializeField] bool drivePreviewRawImage = true;
    [SerializeField] RawImage previewRawImage = null;

    WebCamTexture webCamTexture;
    Color32[] pixelBuffer;
    byte[] arRgbaByteBuffer;
    Texture2D arPreviewTexture;
    float nextScanTime;
    float lastPayloadEmitTime;
    string lastEmittedPayload = string.Empty;
    bool isPreviewActive;
    bool isDecoding;
    bool isStartingCamera;
    bool useArCameraBackend;

    BarcodeReaderGeneric barcodeReader;

    [Header("Debug")]
    [SerializeField] bool logDecodedPayloads = true;

    public event Action<string> OnQrPayloadScanned;

    public bool IsScanning
    {
        get { return isDecoding; }
    }

    public bool IsPreviewActive
    {
        get { return isPreviewActive; }
    }

    public string StatusMessage { get; private set; } = "QR tarayıcı hazır değil";

    void Awake()
    {
        EnsureBarcodeReader();
        ResolveArCameraManager();
    }

    void Start()
    {
        TryBeginScanningWhenReady();
    }

    public void SetPreviewTarget(RawImage rawImage)
    {
        previewRawImage = rawImage;
        drivePreviewRawImage = rawImage != null;
    }

    public void TryBeginScanningWhenReady()
    {
        if (!enabled || !startScanningOnEnable || !ShouldRunLiveScanner())
            return;

        if (isDecoding)
            return;

        StartScanning();
    }

    void EnsureBarcodeReader()
    {
        if (barcodeReader != null)
            return;

        barcodeReader = new BarcodeReaderGeneric
        {
            AutoRotate = true,
            TryInverted = true,
            Options = new ZXing.Common.DecodingOptions
            {
                PossibleFormats = new[] { BarcodeFormat.QR_CODE },
                TryHarder = true
            }
        };
    }

    void ResolveArCameraManager()
    {
        if (arCameraManager != null)
            return;

        arCameraManager = FindAnyObjectByType<ARCameraManager>(FindObjectsInactive.Include);
    }

    bool ShouldRunLiveScanner()
    {
        var configs = FindObjectsByType<DataEnvironmentConfig>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < configs.Length; i++)
        {
            if (configs[i] != null && configs[i].UsesQrLocationExperienceMvp())
                return true;
        }

        var triggerManager = FindAnyObjectByType<LocationTriggerManager>(FindObjectsInactive.Include);
        return triggerManager != null && triggerManager.UsesQrTriggerMode;
    }

    bool PreferWebCamForMvpPreview()
    {
        return drivePreviewRawImage && previewRawImage != null && ShouldRunLiveScanner();
    }

    void OnDisable()
    {
        StopCamera();
    }

    void Update()
    {
        if (!isPreviewActive)
            return;

        if (useArCameraBackend)
            UpdateArPreviewTexture();

        if (!isDecoding)
            return;

        if (Time.unscaledTime < nextScanTime)
            return;

        nextScanTime = Time.unscaledTime + 1f / Mathf.Max(1, targetScanFps);

        if (useArCameraBackend)
            TryDecodeFromArCamera();
        else
            TryDecodeFromWebCam();
    }

    public void StartCameraPreview()
    {
        if (isPreviewActive || isStartingCamera)
            return;

        StartCoroutine(StartCameraRoutine(decodeQr: false));
    }

    public void StartScanning()
    {
        if (isStartingCamera)
            return;

        if (isPreviewActive)
        {
            isDecoding = true;
            StatusMessage = "QR tarama aktif";
            return;
        }

        StartCoroutine(StartCameraRoutine(decodeQr: true));
    }

    public void StopScanning()
    {
        isDecoding = false;
        StatusMessage = isPreviewActive ? "Kamera önizleme aktif" : "QR tarama durduruldu";
    }

    public void StopCamera()
    {
        isDecoding = false;
        isPreviewActive = false;
        isStartingCamera = false;
        useArCameraBackend = false;

        if (webCamTexture != null)
        {
            if (webCamTexture.isPlaying)
                webCamTexture.Stop();

            Destroy(webCamTexture);
            webCamTexture = null;
        }

        if (arPreviewTexture != null)
        {
            Destroy(arPreviewTexture);
            arPreviewTexture = null;
        }

        if (drivePreviewRawImage && previewRawImage != null)
            previewRawImage.texture = null;

        StatusMessage = "Kamera kapatıldı";
    }

    public void SubmitPayloadForTesting(string payload)
    {
        EmitPayload(payload, true);
    }

    IEnumerator StartCameraRoutine(bool decodeQr)
    {
        isStartingCamera = true;
        ResolveArCameraManager();

        yield return RequestCameraPermissionRoutine();

        if (!HasCameraPermission())
        {
            StatusMessage = "Kamera izni verilmedi";
            isStartingCamera = false;
            yield break;
        }

        useArCameraBackend = ShouldUseArCameraBackend();
        if (useArCameraBackend)
        {
            isPreviewActive = true;
            isDecoding = decodeQr;
            isStartingCamera = false;
            StatusMessage = decodeQr ? "QR tarama aktif (AR kamera)" : "Kamera önizleme aktif (AR)";
            yield break;
        }

        if (WebCamTexture.devices == null || WebCamTexture.devices.Length == 0)
        {
            StatusMessage = "Kamera bulunamadı";
            isStartingCamera = false;
            yield break;
        }

        var deviceName = PickRearCameraDeviceName();
        webCamTexture = new WebCamTexture(deviceName, preferredCameraWidth, preferredCameraHeight, 30);
        webCamTexture.Play();

        var waitFrames = 0;
        while (webCamTexture.width <= 16 && waitFrames < 120)
        {
            waitFrames++;
            yield return null;
        }

        ApplyWebCamPreview();

        isPreviewActive = true;
        isDecoding = decodeQr;
        isStartingCamera = false;
        StatusMessage = decodeQr
            ? "QR tarama aktif (WebCam " + webCamTexture.width + "x" + webCamTexture.height + ")"
            : "Kamera önizleme aktif";
    }

    void ApplyWebCamPreview()
    {
        if (!drivePreviewRawImage || previewRawImage == null || webCamTexture == null)
            return;

        previewRawImage.texture = webCamTexture;

        var rotation = webCamTexture.videoRotationAngle;
        previewRawImage.uvRect = webCamTexture.videoVerticallyMirrored
            ? new Rect(0f, 1f, 1f, -1f)
            : new Rect(0f, 0f, 1f, 1f);
        previewRawImage.rectTransform.localEulerAngles = new Vector3(0f, 0f, -rotation);
    }

    bool ShouldUseArCameraBackend()
    {
        if (PreferWebCamForMvpPreview())
            return false;

        if (scanBackend == QrScanBackend.WebCam)
            return false;

        if (scanBackend == QrScanBackend.ArFoundation)
            return arCameraManager != null;

        return arCameraManager != null && arCameraManager.subsystem != null && arCameraManager.subsystem.running;
    }

    static string PickRearCameraDeviceName()
    {
        var devices = WebCamTexture.devices;
        if (devices == null || devices.Length == 0)
            return string.Empty;

        for (var i = 0; i < devices.Length; i++)
        {
            var device = devices[i];
            if (device.isFrontFacing)
                continue;

            return device.name;
        }

        return devices[0].name;
    }

    void UpdateArPreviewTexture()
    {
        if (!drivePreviewRawImage || previewRawImage == null || arCameraManager == null)
            return;

        if (!arCameraManager.TryAcquireLatestCpuImage(out var cpuImage))
            return;

        try
        {
            var conversionParams = new UnityEngine.XR.ARSubsystems.XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
                outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),
                outputFormat = TextureFormat.RGBA32,
                transformation = UnityEngine.XR.ARSubsystems.XRCpuImage.Transformation.MirrorY
            };

            var requiredBytes = cpuImage.GetConvertedDataSize(conversionParams);
            if (arRgbaByteBuffer == null || arRgbaByteBuffer.Length != requiredBytes)
                arRgbaByteBuffer = new byte[requiredBytes];

            using var nativeBuffer = new Unity.Collections.NativeArray<byte>(requiredBytes, Unity.Collections.Allocator.Temp);
            cpuImage.Convert(conversionParams, nativeBuffer);
            nativeBuffer.CopyTo(arRgbaByteBuffer);

            if (arPreviewTexture == null || arPreviewTexture.width != conversionParams.outputDimensions.x ||
                arPreviewTexture.height != conversionParams.outputDimensions.y)
            {
                if (arPreviewTexture != null)
                    Destroy(arPreviewTexture);

                arPreviewTexture = new Texture2D(
                    conversionParams.outputDimensions.x,
                    conversionParams.outputDimensions.y,
                    TextureFormat.RGBA32,
                    false);
            }

            arPreviewTexture.LoadRawTextureData(arRgbaByteBuffer);
            arPreviewTexture.Apply();
            previewRawImage.texture = arPreviewTexture;
        }
        finally
        {
            cpuImage.Dispose();
        }
    }

    void TryDecodeFromArCamera()
    {
        ResolveArCameraManager();
        if (arCameraManager == null)
            return;

        try
        {
            EnsureBarcodeReader();

            if (!ArCameraQrFrameProcessor.TryDecode(barcodeReader, arCameraManager, ref arRgbaByteBuffer, out var decodedText))
                return;

            if (logDecodedPayloads)
                Debug.Log("[QrCodeScanService] QR okundu (AR): " + decodedText);

            EmitPayload(decodedText, false);
        }
        catch (Exception ex)
        {
            StatusMessage = "QR decode hatası (AR): " + ex.Message;
            Debug.LogWarning("[QrCodeScanService] " + StatusMessage);
        }
    }

    void TryDecodeFromWebCam()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying)
            return;

        var width = webCamTexture.width;
        var height = webCamTexture.height;
        if (width <= 16 || height <= 16)
            return;

        var requiredLength = width * height;
        if (pixelBuffer == null || pixelBuffer.Length != requiredLength)
            pixelBuffer = new Color32[requiredLength];

        try
        {
            EnsureBarcodeReader();

            if (!WebCamQrFrameProcessor.TryDecode(barcodeReader, webCamTexture, pixelBuffer, out var decodedText))
                return;

            if (logDecodedPayloads)
                Debug.Log("[QrCodeScanService] QR okundu (WebCam): " + decodedText);

            EmitPayload(decodedText, false);
        }
        catch (Exception ex)
        {
            StatusMessage = "QR decode hatası (WebCam): " + ex.Message;
            Debug.LogWarning("[QrCodeScanService] " + StatusMessage);
        }
    }

    static IEnumerator RequestCameraPermissionRoutine()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            Permission.RequestUserPermission(Permission.Camera);

        var waitTime = 0f;
        while (!Permission.HasUserAuthorizedPermission(Permission.Camera) && waitTime < 8f)
        {
            waitTime += Time.unscaledDeltaTime;
            yield return null;
        }
#else
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
#endif
    }

    static bool HasCameraPermission()
    {
#if UNITY_ANDROID
        return Permission.HasUserAuthorizedPermission(Permission.Camera);
#else
        return Application.HasUserAuthorization(UserAuthorization.WebCam);
#endif
    }

    void EmitPayload(string payload, bool force)
    {
        var normalized = QrPayloadNormalizer.Normalize(payload);
        if (string.IsNullOrEmpty(normalized))
            return;

        if (!force)
        {
            if (Time.unscaledTime - lastPayloadEmitTime < rescanCooldownSeconds &&
                normalized == lastEmittedPayload)
                return;
        }

        lastEmittedPayload = normalized;
        lastPayloadEmitTime = Time.unscaledTime;
        StatusMessage = "QR okundu: " + normalized;
        OnQrPayloadScanned?.Invoke(normalized);
    }

    void OnEnable()
    {
        TryBeginScanningWhenReady();
    }
}

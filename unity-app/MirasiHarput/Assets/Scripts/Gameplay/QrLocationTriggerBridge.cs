using System;
using System.Collections;
using UnityEngine;

public class QrLocationTriggerBridge : MonoBehaviour
{
    [SerializeField] JsonDataLoader dataLoader = null;
    [SerializeField] LocationTriggerManager locationTriggerManager = null;
    [SerializeField] QrCodeScanService qrScanService = null;

    [Header("Davranış")]
    [SerializeField] bool autoStartCameraOnLoad = false;
    [SerializeField] bool logUnrecognizedPayloads = true;
    [SerializeField] bool logSuccessfulTriggers = true;

    public event Action<QrScanAttemptResult, string, LocationData> OnQrScanAttempt;

    public string StatusMessage { get; private set; } = "QR tetik köprüsü bekleniyor";

    Coroutine initializeRoutine;

    void OnEnable()
    {
        ResolveReferences();
        Subscribe();
        BeginInitializeRoutine();
    }

    void Start()
    {
        ResolveReferences();
        Subscribe();
        BeginInitializeRoutine();
    }

    void OnDisable()
    {
        if (initializeRoutine != null)
        {
            StopCoroutine(initializeRoutine);
            initializeRoutine = null;
        }

        Unsubscribe();
    }

    public void BeginInitializeRoutine()
    {
        if (initializeRoutine != null)
            return;

        initializeRoutine = StartCoroutine(InitializeQrPipelineRoutine());
    }

    void ResolveReferences()
    {
        if (dataLoader == null)
            dataLoader = JsonDataLoader.Instance != null ? JsonDataLoader.Instance : FindAnyObjectByType<JsonDataLoader>();

        if (locationTriggerManager == null)
            locationTriggerManager = FindAnyObjectByType<LocationTriggerManager>();

        if (qrScanService == null)
            qrScanService = FindAnyObjectByType<QrCodeScanService>();
    }

    void Subscribe()
    {
        if (qrScanService != null)
        {
            qrScanService.OnQrPayloadScanned -= HandleQrPayloadScanned;
            qrScanService.OnQrPayloadScanned += HandleQrPayloadScanned;
        }

        if (dataLoader != null)
        {
            dataLoader.OnDataLoaded -= HandleDataLoaded;
            dataLoader.OnDataLoaded += HandleDataLoaded;
        }
    }

    void Unsubscribe()
    {
        if (qrScanService != null)
            qrScanService.OnQrPayloadScanned -= HandleQrPayloadScanned;

        if (dataLoader != null)
            dataLoader.OnDataLoaded -= HandleDataLoaded;
    }

    void HandleDataLoaded(JsonDataLoader _)
    {
        ResolveReferences();
        StatusMessage = dataLoader != null && dataLoader.HasQrRegistry
            ? "QR kayıt defteri yüklendi (" + dataLoader.QrRegistryEntryCount + " mekan)"
            : "QR kayıt defteri yok";

        BeginInitializeRoutine();
    }

    IEnumerator InitializeQrPipelineRoutine()
    {
        const float timeoutSeconds = 12f;
        var elapsed = 0f;

        while (elapsed < timeoutSeconds)
        {
            ResolveReferences();
            Subscribe();

            var triggerReady = locationTriggerManager != null && locationTriggerManager.UsesQrTriggerMode;
            var dataReady = dataLoader != null && dataLoader.IsLoaded && dataLoader.HasQrRegistry;

            if (triggerReady && dataReady)
            {
                if (autoStartCameraOnLoad && qrScanService != null)
                    qrScanService.TryBeginScanningWhenReady();

                StatusMessage = "QR pipeline hazır";
                initializeRoutine = null;
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        StatusMessage = "QR pipeline hazır değil (mod veya data bekleniyor)";
        initializeRoutine = null;
    }

    public bool TryTriggerFromPayload(string rawPayload)
    {
        return TryTriggerFromPayload(rawPayload, out _, out _) == QrScanAttemptResult.Success;
    }

    public QrScanAttemptResult TryTriggerFromPayload(string rawPayload, out string message, out LocationData location)
    {
        message = string.Empty;
        location = null;
        ResolveReferences();

        if (locationTriggerManager == null)
        {
            message = "LocationTriggerManager bulunamadı";
            Report(QrScanAttemptResult.TriggerRejected, message, null);
            return QrScanAttemptResult.TriggerRejected;
        }

        if (!locationTriggerManager.UsesQrTriggerMode)
        {
            message = "Tetikleme modu QR değil";
            Report(QrScanAttemptResult.NotQrMode, message, null);
            return QrScanAttemptResult.NotQrMode;
        }

        if (dataLoader == null || !dataLoader.IsLoaded)
        {
            message = "Veri yükleniyor, lütfen tekrar deneyin.";
            Report(QrScanAttemptResult.DataNotReady, message, null);
            return QrScanAttemptResult.DataNotReady;
        }

        var normalized = QrPayloadNormalizer.Normalize(rawPayload);
        if (QrPayloadNormalizer.IsDynamicRedirectHost(normalized))
        {
            message = "Bu QR dinamik bir link. Statik mirasi-harput QR kullanın.";
            if (logUnrecognizedPayloads)
                Debug.LogWarning("[QrLocationTriggerBridge] " + message + " | ham: " + rawPayload);
            Report(QrScanAttemptResult.DynamicRedirectQr, message, null);
            return QrScanAttemptResult.DynamicRedirectQr;
        }

        if (!dataLoader.TryResolveLocationFromQrPayload(rawPayload, out location, out _))
        {
            message = "Tanınmayan QR kodu.";
            if (logUnrecognizedPayloads)
                Debug.LogWarning("[QrLocationTriggerBridge] Tanınmayan QR: " + normalized);
            Report(QrScanAttemptResult.UnknownQr, message, null);
            return QrScanAttemptResult.UnknownQr;
        }

        if (!locationTriggerManager.TryValidateQrTrigger(location, out var validationMessage))
        {
            message = validationMessage;
            var isWrongOrder = locationTriggerManager.CurrentTargetLocation != null &&
                location.id != locationTriggerManager.CurrentTargetLocation.id;
            var result = isWrongOrder ? QrScanAttemptResult.WrongRouteOrder : QrScanAttemptResult.AlreadyTriggered;
            if (logUnrecognizedPayloads)
                Debug.LogWarning("[QrLocationTriggerBridge] " + message);
            Report(result, message, location);
            return result;
        }

        locationTriggerManager.TriggerLocation(location);

        if (locationTriggerManager.ActiveLocation != null && locationTriggerManager.ActiveLocation.id == location.id)
        {
            message = location.name + " — NPC hazır.";
            StatusMessage = "QR ile tetiklendi: " + location.name;
            if (logSuccessfulTriggers)
                Debug.Log("[QrLocationTriggerBridge] " + StatusMessage);
            Report(QrScanAttemptResult.Success, message, location);
            return QrScanAttemptResult.Success;
        }

        message = locationTriggerManager.StatusMessage;
        Report(QrScanAttemptResult.TriggerRejected, message, location);
        return QrScanAttemptResult.TriggerRejected;
    }

    void HandleQrPayloadScanned(string payload)
    {
        if (logSuccessfulTriggers)
            Debug.Log("[QrLocationTriggerBridge] Payload alındı: " + payload);

        TryTriggerFromPayload(payload, out _, out _);
    }

    void Report(QrScanAttemptResult result, string message, LocationData location)
    {
        StatusMessage = message;
        OnQrScanAttempt?.Invoke(result, message, location);
    }
}

using UnityEngine;
using UnityEngine.UI;

public enum AppExperienceState
{
    QrReader = 0,
    LocationExperienceAudioInactive = 1,
    LocationExperienceAudioActive = 2
}

[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(LocationAppUIView))]
public class AppFlowController : MonoBehaviour
{
    [SerializeField] LocationAppUIView view = null;
    [SerializeField] QrCodeScanService qrScanService = null;
    [SerializeField] AudioController audioController = null;
    [SerializeField] ARModelViewer modelViewer = null;

    AppExperienceState currentState = AppExperienceState.QrReader;
    LocationModel activeLocation;
    bool uiWired;
    bool audioPrepared;

    public AppExperienceState CurrentState
    {
        get { return currentState; }
    }

    void Awake()
    {
        ResolveReferences();
        WireUiOnce();
        HideLegacyPanels();
    }

    void OnEnable()
    {
        ResolveReferences();
        WireUiOnce();
        Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
        StopExperienceResources();
    }

    void Start()
    {
        ResolveReferences();
        EnsureQrScanService();
        Subscribe();
        DataEnvironmentModeActivator.EnsureQrRouteUiVisible();
        ShowQrReader();
    }

    void Update()
    {
        if (currentState != AppExperienceState.QrReader || qrScanService == null || view == null)
            return;

        if (view.ScanStatusText != null && !string.IsNullOrEmpty(qrScanService.StatusMessage))
            view.ScanStatusText.text = qrScanService.StatusMessage;

        if (qrScanService.StatusMessage != null &&
            qrScanService.StatusMessage.IndexOf("izni", System.StringComparison.OrdinalIgnoreCase) >= 0)
            view.ShowToast("Kamera izni olmadan QR okuma kullanılamaz.");
    }

    void EnsureQrScanService()
    {
        if (qrScanService != null && qrScanService.isActiveAndEnabled)
            return;

        var services = FindObjectsByType<QrCodeScanService>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < services.Length; i++)
        {
            if (services[i] != null && services[i].isActiveAndEnabled)
            {
                qrScanService = services[i];
                return;
            }
        }

        var scanHost = new GameObject("QrCodeScanService");
        qrScanService = scanHost.AddComponent<QrCodeScanService>();
    }

    void ResolveReferences()
    {
        if (view == null)
            view = GetComponent<LocationAppUIView>();

        if (qrScanService == null || !qrScanService.isActiveAndEnabled)
            EnsureQrScanService();

        if (audioController == null)
            audioController = GetComponent<AudioController>();

        if (audioController == null)
            audioController = gameObject.AddComponent<AudioController>();

        if (modelViewer == null)
            modelViewer = FindAnyObjectByType<ARModelViewer>();

        if (modelViewer == null)
        {
            var modelHost = new GameObject("LocationModelViewer");
            modelViewer = modelHost.AddComponent<ARModelViewer>();
        }
    }

    void WireUiOnce()
    {
        if (uiWired || view == null)
            return;

        uiWired = true;
        BindButton(view.ScanButton, OnScanButtonPressed);
        BindButton(view.BackButton, ReturnToQrReader);
        BindButton(view.AudioToggleButton, ToggleAudio);
    }

    static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    void Subscribe()
    {
        if (qrScanService != null)
        {
            qrScanService.OnQrPayloadScanned -= HandleQrPayloadScanned;
            qrScanService.OnQrPayloadScanned += HandleQrPayloadScanned;
        }

        if (audioController != null)
        {
            audioController.OnPlayingStateChanged -= HandleAudioPlayingStateChanged;
            audioController.OnPlayingStateChanged += HandleAudioPlayingStateChanged;
            audioController.OnPlaybackCompleted -= HandleAudioPlaybackCompleted;
            audioController.OnPlaybackCompleted += HandleAudioPlaybackCompleted;
        }
    }

    void Unsubscribe()
    {
        if (qrScanService != null)
            qrScanService.OnQrPayloadScanned -= HandleQrPayloadScanned;

        if (audioController != null)
        {
            audioController.OnPlayingStateChanged -= HandleAudioPlayingStateChanged;
            audioController.OnPlaybackCompleted -= HandleAudioPlaybackCompleted;
        }
    }

    void HideLegacyPanels()
    {
        SetPanelActive("LocationDebugPanel", false);
        SetPanelActive("LocationTriggerDebugPanel", false);
        SetPanelActive("DataDebugPanel", false);
        SetPanelActive("QuestProgressDebugPanel", false);
        SetPanelActive("QuestInteractionPanel", false);
        SetPanelActive("IndoorNpcSetupPanel", false);
        SetPanelActive("IndoorNpcInteractionPanel", false);
    }

    void ShowQrReader()
    {
        currentState = AppExperienceState.QrReader;
        activeLocation = null;
        audioPrepared = false;

        if (view == null)
            return;

        transform.SetAsLastSibling();
        SetActive(view.QrReaderScreen, true);
        SetActive(view.LocationExperienceScreen, false);
        view.SetAudioButtonActiveState(false);

        if (view.ScanStatusText != null)
            view.ScanStatusText.text = string.Empty;

        StartQrCameraPreview();
    }

    void StartQrCameraPreview()
    {
        EnsureQrScanService();
        Subscribe();

        if (qrScanService == null || view == null)
            return;

        if (view.CameraPreview != null)
            qrScanService.SetPreviewTarget(view.CameraPreview);

        qrScanService.StartCameraPreview();
    }

    void OnScanButtonPressed()
    {
        EnsureQrScanService();
        Subscribe();

        if (qrScanService == null)
        {
            view.ShowToast("Kamera izni olmadan QR okuma kullanılamaz.");
            return;
        }

        if (view.CameraPreview != null)
            qrScanService.SetPreviewTarget(view.CameraPreview);

        if (view.ScanStatusText != null)
            view.ScanStatusText.text = "Taranıyor… QR kodu çerçeveye hizalayın.";

        qrScanService.StartScanning();
    }

    void HandleQrPayloadScanned(string payload)
    {
        if (currentState != AppExperienceState.QrReader || view == null)
            return;

        if (!QRPayloadParser.TryParse(payload, out var locationId, out var parseError))
        {
            view.ShowToast(parseError);
            return;
        }

        if (!LocationRepository.TryGetById(locationId, out var location))
        {
            view.ShowToast("Bu QR kod için mekan içeriği bulunamadı.");
            return;
        }

        OpenLocationExperience(location);
    }

    void OpenLocationExperience(LocationModel location)
    {
        if (location == null)
            return;

        if (qrScanService != null)
            qrScanService.StopCamera();

        StopExperienceResources();
        activeLocation = location;
        audioPrepared = audioController != null && audioController.TryPrepare(location.audioPath);

        var modelLoaded = modelViewer != null && modelViewer.TryShowLocation(location);
        if (!modelLoaded && view != null)
            view.ShowToast("3D model şu anda yüklenemedi.");

        if (view.DescriptionText != null)
            view.DescriptionText.text = location.description;

        if (view.DescriptionScrollRect != null)
            view.DescriptionScrollRect.verticalNormalizedPosition = 1f;

        view.SetAudioButtonActiveState(false);
        currentState = AppExperienceState.LocationExperienceAudioInactive;

        SetActive(view.QrReaderScreen, false);
        SetActive(view.LocationExperienceScreen, true);
        transform.SetAsLastSibling();
    }

    void ToggleAudio()
    {
        if (activeLocation == null || audioController == null || view == null)
            return;

        if (audioController.IsPlaying)
        {
            audioController.PauseOrStop();
            currentState = AppExperienceState.LocationExperienceAudioInactive;
            return;
        }

        if (!audioPrepared && !audioController.TryPrepare(activeLocation.audioPath))
        {
            view.ShowToast("Seslendirme şu anda oynatılamıyor.");
            return;
        }

        audioPrepared = true;
        if (!audioController.TryPlay())
        {
            view.ShowToast("Seslendirme şu anda oynatılamıyor.");
            return;
        }

        currentState = AppExperienceState.LocationExperienceAudioActive;
    }

    void HandleAudioPlayingStateChanged(bool isPlaying)
    {
        if (view != null)
            view.SetAudioButtonActiveState(isPlaying);
    }

    void HandleAudioPlaybackCompleted()
    {
        currentState = AppExperienceState.LocationExperienceAudioInactive;
        if (view != null)
            view.SetAudioButtonActiveState(false);
    }

    void ReturnToQrReader()
    {
        StopExperienceResources();
        ShowQrReader();
    }

    void StopExperienceResources()
    {
        if (audioController != null)
            audioController.StopAndClear();

        if (modelViewer != null)
            modelViewer.ClearModel();
    }

    static void SetActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    static void SetPanelActive(string panelName, bool active)
    {
        if (string.IsNullOrEmpty(panelName))
            return;

        var panels = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null && panels[i].name == panelName)
            {
                panels[i].gameObject.SetActive(active);
                return;
            }
        }
    }
}

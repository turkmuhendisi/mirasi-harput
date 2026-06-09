using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum QrFlowScreen
{
    Welcome = 0,
    RouteSelect = 1,
    StartConfirm = 2,
    QrHub = 3,
    QrScan = 4,
    ArExperience = 5
}

/// <summary>
/// QR rota akışının mantığı. Görsel tasarım <see cref="QrFlowUIView"/> + prefab/sahne ile yapılır.
/// </summary>
[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(QrFlowUIView))]
public class QrFlowController : MonoBehaviour
{
    [SerializeField] QrFlowUIView view = null;
    [SerializeField] JsonDataLoader dataLoader = null;
    [SerializeField] LocationTriggerManager locationTriggerManager = null;
    [SerializeField] QrLocationTriggerBridge qrBridge = null;
    [SerializeField] QrCodeScanService qrScanService = null;
    [SerializeField] QuestProgressManager questProgressManager = null;

    [Header("Başlangıç")]
    [SerializeField] bool hideLegacyPanelsOnStart = true;

    readonly List<QrRouteListItem> routeListItems = new List<QrRouteListItem>();
    RouteOrderData selectedRoute;
    QrFlowScreen currentScreen = QrFlowScreen.Welcome;
    bool uiWired;

    public QrFlowScreen CurrentScreen
    {
        get { return currentScreen; }
    }

    void Awake()
    {
        ResolveReferences();
        WireUiOnce();
        ApplyInitialVisibility();
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
    }

    void Start()
    {
        ResolveReferences();
        DataEnvironmentModeActivator.EnsureQrRouteUiVisible();

        if (view != null && !view.HasRequiredScreens())
            Debug.LogWarning("[QrFlowController] Ekran referansları eksik. QrFlowUIView üzerinde Auto Wire çalıştırın veya prefab'ı yeniden oluşturun.");

        if (dataLoader != null && !dataLoader.IsLoaded)
            dataLoader.OnDataLoaded += HandleDataLoadedOnce;
        else
            RefreshRouteList();

        ShowScreen(QrFlowScreen.Welcome);
    }

    void Update()
    {
        if (currentScreen != QrFlowScreen.QrScan || view == null || view.ScanStatusText == null || qrScanService == null)
            return;

        if (qrScanService.IsScanning)
            view.ScanStatusText.text = qrScanService.StatusMessage;
    }

    void ResolveReferences()
    {
        if (view == null)
            view = GetComponent<QrFlowUIView>();

        if (dataLoader == null)
            dataLoader = JsonDataLoader.Instance != null ? JsonDataLoader.Instance : FindAnyObjectByType<JsonDataLoader>();

        if (locationTriggerManager == null)
            locationTriggerManager = FindAnyObjectByType<LocationTriggerManager>();

        if (qrBridge == null)
            qrBridge = FindAnyObjectByType<QrLocationTriggerBridge>();

        if (qrScanService == null)
            qrScanService = FindAnyObjectByType<QrCodeScanService>();

        if (questProgressManager == null)
            questProgressManager = FindAnyObjectByType<QuestProgressManager>();
    }

    void WireUiOnce()
    {
        if (uiWired || view == null)
            return;

        uiWired = true;

        BindButton(view.WelcomeContinueButton ?? FindButtonOnScreen(view.WelcomeScreen, "ContinueButton"),
            () => ShowScreen(QrFlowScreen.RouteSelect));
        BindButton(view.RouteSelectBackButton ?? FindButtonOnScreen(view.RouteSelectScreen, "BackButton"),
            () => ShowScreen(QrFlowScreen.Welcome));
        BindButton(view.ConfirmCancelButton, () => ShowScreen(QrFlowScreen.RouteSelect));
        BindButton(view.ConfirmStartButton, ConfirmRouteStart);
        BindButton(view.HubScanButton, BeginQrScan);
        BindButton(view.HubChangeRouteButton, () => ShowScreen(QrFlowScreen.RouteSelect));
        BindButton(view.ScanCloseButton, CancelQrScan);
        BindButton(view.ScanBackToHubButton, CancelQrScan);
        BindButton(view.ExperienceQuestButton, OpenQuestPanel);
        BindButton(view.ExperienceBackToHubButton, ReturnToHubAfterExperience);
    }

    static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    static Button FindButtonOnScreen(GameObject screen, string childName)
    {
        if (screen == null)
            return null;

        var t = screen.transform.Find(childName);
        return t != null ? t.GetComponent<Button>() : null;
    }

    void Subscribe()
    {
        if (qrBridge != null)
        {
            qrBridge.OnQrScanAttempt -= HandleQrScanAttempt;
            qrBridge.OnQrScanAttempt += HandleQrScanAttempt;
        }

        if (questProgressManager != null)
        {
            questProgressManager.OnQuestCompleted -= HandleQuestCompleted;
            questProgressManager.OnQuestCompleted += HandleQuestCompleted;
        }
    }

    void Unsubscribe()
    {
        if (qrBridge != null)
            qrBridge.OnQrScanAttempt -= HandleQrScanAttempt;

        if (questProgressManager != null)
            questProgressManager.OnQuestCompleted -= HandleQuestCompleted;

        if (dataLoader != null)
            dataLoader.OnDataLoaded -= HandleDataLoadedOnce;
    }

    void HandleQuestCompleted(QuestData _, int __, string ___)
    {
        SetPanelActive(view != null ? view.QuestInteractionPanelName : "QuestInteractionPanel", false);
        NotifyQuestCompleted();
        ShowScreen(QrFlowScreen.QrHub);
    }

    void HandleDataLoadedOnce(JsonDataLoader _)
    {
        dataLoader.OnDataLoaded -= HandleDataLoadedOnce;
        RefreshRouteList();
    }

    void ApplyInitialVisibility()
    {
        if (!hideLegacyPanelsOnStart)
            return;

        SetPanelActive("LocationDebugPanel", false);
        SetPanelActive("LocationTriggerDebugPanel", false);
        SetPanelActive("DataDebugPanel", false);
        SetPanelActive("QuestProgressDebugPanel", false);
        SetPanelActive(view != null ? view.QuestInteractionPanelName : "QuestInteractionPanel", false);
    }

    public void ShowScreen(QrFlowScreen screen)
    {
        if (view == null)
            return;

        currentScreen = screen;
        transform.SetAsLastSibling();

        SetActive(view.WelcomeScreen, screen == QrFlowScreen.Welcome);
        SetActive(view.RouteSelectScreen, screen == QrFlowScreen.RouteSelect);
        SetActive(view.StartConfirmModal, screen == QrFlowScreen.StartConfirm);
        SetActive(view.QrHubScreen, screen == QrFlowScreen.QrHub);
        SetActive(view.QrScanScreen, screen == QrFlowScreen.QrScan);
        SetActive(view.ArExperienceScreen, screen == QrFlowScreen.ArExperience);

        if (screen != QrFlowScreen.QrScan && qrScanService != null)
            qrScanService.StopScanning();
    }

    static void SetActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }

    void RefreshRouteList()
    {
        ResolveReferences();
        ClearRouteList();

        if (view == null || view.RouteButtonContainer == null || dataLoader == null || !dataLoader.IsLoaded)
        {
            if (view != null && view.RouteListHintText != null)
                view.RouteListHintText.text = "Rota verisi yükleniyor…";
            return;
        }

        if (dataLoader.RouteOrder.Count == 0)
        {
            if (view.RouteListHintText != null)
                view.RouteListHintText.text = "Tanımlı rota bulunamadı.";
            return;
        }

        if (view.RouteListHintText != null)
            view.RouteListHintText.text = "Bir rota seçin:";

        if (view.RouteListItemPrefab == null)
        {
            Debug.LogWarning("[QrFlowController] routeListItemPrefab atanmadı. QrFlowUIView Inspector'ından atayın.");
            return;
        }

        for (var i = 0; i < dataLoader.RouteOrder.Count; i++)
        {
            var route = dataLoader.RouteOrder[i];
            if (route == null)
                continue;

            CreateRouteListItem(route);
        }
    }

    void ClearRouteList()
    {
        for (var i = 0; i < routeListItems.Count; i++)
        {
            if (routeListItems[i] != null)
                Destroy(routeListItems[i].gameObject);
        }

        routeListItems.Clear();
    }

    void CreateRouteListItem(RouteOrderData route)
    {
        var label = string.IsNullOrEmpty(route.routeName) ? route.routeId : route.routeName;
        var item = Instantiate(view.RouteListItemPrefab, view.RouteButtonContainer);
        item.gameObject.SetActive(true);
        var capturedRoute = route;
        item.Setup(label, () => SelectRoute(capturedRoute));
        routeListItems.Add(item);
    }

    void SelectRoute(RouteOrderData route)
    {
        selectedRoute = route;

        if (view.ConfirmRouteNameText != null)
            view.ConfirmRouteNameText.text = route.routeName;

        var stopCount = route.orderedLocationIds != null ? route.orderedLocationIds.Length : 0;
        if (view.ConfirmBodyText != null)
            view.ConfirmBodyText.text = stopCount + " duraklı bu rotayı başlatmak istiyor musunuz?";

        ShowScreen(QrFlowScreen.StartConfirm);
    }

    void ConfirmRouteStart()
    {
        ResolveReferences();
        if (locationTriggerManager == null || dataLoader == null || !dataLoader.IsLoaded)
            return;

        locationTriggerManager.ResetRouteProgress();
        locationTriggerManager.InitializeAfterDataLoaded();

        if (selectedRoute != null &&
            selectedRoute.orderedLocationIds != null &&
            selectedRoute.orderedLocationIds.Length > 0)
            locationTriggerManager.SetCurrentTargetByLocationId(selectedRoute.orderedLocationIds[0]);

        RefreshHubTexts();
        ShowScreen(QrFlowScreen.QrHub);
    }

    void RefreshHubTexts()
    {
        ResolveReferences();
        if (view == null)
            return;

        if (view.HubRouteNameText != null)
            view.HubRouteNameText.text = selectedRoute != null ? selectedRoute.routeName : "Rota";

        if (locationTriggerManager == null || dataLoader == null)
            return;

        var target = locationTriggerManager.CurrentTargetLocation;
        var progress = locationTriggerManager.GetRouteProgressText();

        if (view.HubCheckpointText != null)
            view.HubCheckpointText.text = "Durak " + progress;

        if (view.HubTargetText != null)
            view.HubTargetText.text = target != null ? "Hedef: " + target.name : "Hedef: —";

        if (view.HubNextCheckpointText != null)
        {
            var next = target != null ? dataLoader.GetNextLocation(target.id) : null;
            view.HubNextCheckpointText.text = next != null ? "Sonraki durak: " + next.name : "Son durak";
        }

        if (view.HubStatusText != null)
            view.HubStatusText.text = "Sıradaki duraktaki QR kodunu okutmak için butona basın.";

        if (view.ExperienceRouteText != null)
            view.ExperienceRouteText.text = view.HubRouteNameText.text + "  •  " +
                (view.HubCheckpointText != null ? view.HubCheckpointText.text : string.Empty) + "  •  " +
                (view.HubTargetText != null ? view.HubTargetText.text : string.Empty);
    }

    void BeginQrScan()
    {
        RefreshHubTexts();
        ShowScreen(QrFlowScreen.QrScan);

        if (view != null && view.ScanStatusText != null)
            view.ScanStatusText.text = "Kamera açılıyor… QR kodu hedefe hizalayın.";

        if (qrScanService != null)
            qrScanService.StartScanning();
    }

    void CancelQrScan()
    {
        if (qrScanService != null)
            qrScanService.StopScanning();

        ShowScreen(QrFlowScreen.QrHub);
    }

    void HandleQrScanAttempt(QrScanAttemptResult result, string message, LocationData location)
    {
        if (currentScreen != QrFlowScreen.QrScan || view == null)
            return;

        if (view.ScanStatusText != null)
            view.ScanStatusText.text = message;

        if (result == QrScanAttemptResult.Success)
        {
            if (qrScanService != null)
                qrScanService.StopScanning();

            if (view.ExperienceStatusText != null)
                view.ExperienceStatusText.text = message;

            RefreshHubTexts();
            ShowScreen(QrFlowScreen.ArExperience);
            return;
        }

        if (result == QrScanAttemptResult.WrongRouteOrder && view.HubStatusText != null)
            view.HubStatusText.text = message;
    }

    void OpenQuestPanel()
    {
        var panelName = view != null ? view.QuestInteractionPanelName : "QuestInteractionPanel";
        SetPanelActive(panelName, true);

        if (locationTriggerManager != null && locationTriggerManager.ActiveQuest != null && questProgressManager != null)
        {
            questProgressManager.HandleLocationTriggered(
                locationTriggerManager.ActiveLocation,
                locationTriggerManager.ActiveQuest);
        }
    }

    void ReturnToHubAfterExperience()
    {
        RefreshHubTexts();
        ShowScreen(QrFlowScreen.QrHub);
    }

    public void NotifyQuestCompleted()
    {
        RefreshHubTexts();
        if (view != null && view.HubStatusText != null)
            view.HubStatusText.text = "Görev tamamlandı. Sonraki durağın QR kodunu okutun.";
    }

    static void SetPanelActive(string panelName, bool active)
    {
        if (string.IsNullOrEmpty(panelName))
            return;

        var panels = UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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

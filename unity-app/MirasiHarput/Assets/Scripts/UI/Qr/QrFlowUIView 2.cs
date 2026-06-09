using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// QR akışı arayüz referansları. Hierarchy'yi Unity Editor'da tasarlayın;
/// bu bileşendeki alanları Inspector'dan sürükleyip bırakın veya "Auto Wire" kullanın.
/// </summary>
[DisallowMultipleComponent]
public class QrFlowUIView : MonoBehaviour
{
    public const string PrefabResourcesPath = "UI/Qr/QrFlowUI";

    [Header("Ekran kökleri")]
    [SerializeField] GameObject welcomeScreen = null;
    [SerializeField] GameObject routeSelectScreen = null;
    [SerializeField] GameObject startConfirmModal = null;
    [SerializeField] GameObject qrHubScreen = null;
    [SerializeField] GameObject qrScanScreen = null;
    [SerializeField] GameObject arExperienceScreen = null;

    [Header("Karşılama")]
    [SerializeField] Button welcomeContinueButton = null;

    [Header("Rota seçimi")]
    [SerializeField] TMP_Text routeListHintText = null;
    [SerializeField] Transform routeButtonContainer = null;
    [SerializeField] Button routeSelectBackButton = null;
    [SerializeField] QrRouteListItem routeListItemPrefab = null;

    [Header("Rota onay")]
    [SerializeField] TMP_Text confirmRouteNameText = null;
    [SerializeField] TMP_Text confirmBodyText = null;
    [SerializeField] Button confirmCancelButton = null;
    [SerializeField] Button confirmStartButton = null;

    [Header("QR hub")]
    [SerializeField] TMP_Text hubRouteNameText = null;
    [SerializeField] TMP_Text hubCheckpointText = null;
    [SerializeField] TMP_Text hubTargetText = null;
    [SerializeField] TMP_Text hubNextCheckpointText = null;
    [SerializeField] TMP_Text hubStatusText = null;
    [SerializeField] Button hubScanButton = null;
    [SerializeField] Button hubChangeRouteButton = null;

    [Header("QR tarama")]
    [SerializeField] TMP_Text scanHintText = null;
    [SerializeField] TMP_Text scanStatusText = null;
    [SerializeField] Button scanCloseButton = null;
    [SerializeField] Button scanBackToHubButton = null;

    [Header("AR deneyim")]
    [SerializeField] TMP_Text experienceRouteText = null;
    [SerializeField] TMP_Text experienceStatusText = null;
    [SerializeField] Button experienceQuestButton = null;
    [SerializeField] Button experienceBackToHubButton = null;

    [Header("Harici paneller")]
    [SerializeField] string questInteractionPanelName = "QuestInteractionPanel";

    public GameObject WelcomeScreen
    {
        get { return welcomeScreen; }
    }

    public GameObject RouteSelectScreen
    {
        get { return routeSelectScreen; }
    }

    public GameObject StartConfirmModal
    {
        get { return startConfirmModal; }
    }

    public GameObject QrHubScreen
    {
        get { return qrHubScreen; }
    }

    public GameObject QrScanScreen
    {
        get { return qrScanScreen; }
    }

    public GameObject ArExperienceScreen
    {
        get { return arExperienceScreen; }
    }

    public TMP_Text RouteListHintText
    {
        get { return routeListHintText; }
    }

    public Transform RouteButtonContainer
    {
        get { return routeButtonContainer; }
    }

    public QrRouteListItem RouteListItemPrefab
    {
        get { return routeListItemPrefab; }
    }

    public TMP_Text ConfirmRouteNameText
    {
        get { return confirmRouteNameText; }
    }

    public TMP_Text ConfirmBodyText
    {
        get { return confirmBodyText; }
    }

    public TMP_Text HubRouteNameText
    {
        get { return hubRouteNameText; }
    }

    public TMP_Text HubCheckpointText
    {
        get { return hubCheckpointText; }
    }

    public TMP_Text HubTargetText
    {
        get { return hubTargetText; }
    }

    public TMP_Text HubNextCheckpointText
    {
        get { return hubNextCheckpointText; }
    }

    public TMP_Text HubStatusText
    {
        get { return hubStatusText; }
    }

    public TMP_Text ScanHintText
    {
        get { return scanHintText; }
    }

    public TMP_Text ScanStatusText
    {
        get { return scanStatusText; }
    }

    public TMP_Text ExperienceRouteText
    {
        get { return experienceRouteText; }
    }

    public TMP_Text ExperienceStatusText
    {
        get { return experienceStatusText; }
    }

    public Button WelcomeContinueButton
    {
        get { return welcomeContinueButton; }
    }

    public Button RouteSelectBackButton
    {
        get { return routeSelectBackButton; }
    }

    public Button ConfirmCancelButton
    {
        get { return confirmCancelButton; }
    }

    public Button ConfirmStartButton
    {
        get { return confirmStartButton; }
    }

    public Button HubScanButton
    {
        get { return hubScanButton; }
    }

    public Button HubChangeRouteButton
    {
        get { return hubChangeRouteButton; }
    }

    public Button ScanCloseButton
    {
        get { return scanCloseButton; }
    }

    public Button ScanBackToHubButton
    {
        get { return scanBackToHubButton; }
    }

    public Button ExperienceQuestButton
    {
        get { return experienceQuestButton; }
    }

    public Button ExperienceBackToHubButton
    {
        get { return experienceBackToHubButton; }
    }

    public string QuestInteractionPanelName
    {
        get { return questInteractionPanelName; }
    }

    public bool HasRequiredScreens()
    {
        return welcomeScreen != null &&
            routeSelectScreen != null &&
            startConfirmModal != null &&
            qrHubScreen != null &&
            qrScanScreen != null &&
            arExperienceScreen != null;
    }

#if UNITY_EDITOR
    [ContextMenu("Auto Wire (standart isimler)")]
    void AutoWireContextMenu()
    {
        AutoWireByName();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    public void AutoWireByName()
    {
        welcomeScreen = FindChild(welcomeScreen, "WelcomeScreen");
        routeSelectScreen = FindChild(routeSelectScreen, "RouteSelectScreen");
        startConfirmModal = FindChild(startConfirmModal, "StartConfirmModal");
        qrHubScreen = FindChild(qrHubScreen, "QrHubScreen");
        qrScanScreen = FindChild(qrScanScreen, "QrScanScreen");
        arExperienceScreen = FindChild(arExperienceScreen, "ArExperienceScreen");

        welcomeContinueButton = FindButton(welcomeContinueButton, welcomeScreen, "ContinueButton");
        if (welcomeContinueButton == null && welcomeScreen != null)
            welcomeContinueButton = welcomeScreen.GetComponentInChildren<Button>(true);

        routeListHintText = FindText(routeListHintText, routeSelectScreen, "Hint");
        routeButtonContainer = FindTransform(routeButtonContainer, routeSelectScreen, "RouteScroll/Viewport/Content");
        routeSelectBackButton = FindButton(routeSelectBackButton, routeSelectScreen, "BackButton");

        confirmRouteNameText = FindText(confirmRouteNameText, startConfirmModal, "Card/RouteName");
        confirmBodyText = FindText(confirmBodyText, startConfirmModal, "Card/Body");
        confirmCancelButton = FindButton(confirmCancelButton, startConfirmModal, "Card/Cancel");
        confirmStartButton = FindButton(confirmStartButton, startConfirmModal, "Card/Start");

        hubRouteNameText = FindText(hubRouteNameText, qrHubScreen, "RouteName");
        hubCheckpointText = FindText(hubCheckpointText, qrHubScreen, "Checkpoint");
        hubTargetText = FindText(hubTargetText, qrHubScreen, "Target");
        hubNextCheckpointText = FindText(hubNextCheckpointText, qrHubScreen, "Next");
        hubStatusText = FindText(hubStatusText, qrHubScreen, "Status");
        hubScanButton = FindButton(hubScanButton, qrHubScreen, "ScanButton");
        hubChangeRouteButton = FindButton(hubChangeRouteButton, qrHubScreen, "ChangeRoute");

        scanHintText = FindText(scanHintText, qrScanScreen, "TopBar/Hint");
        scanStatusText = FindText(scanStatusText, qrScanScreen, "BottomBar/Status");
        scanCloseButton = FindButton(scanCloseButton, qrScanScreen, "TopBar/Close");
        scanBackToHubButton = FindButton(scanBackToHubButton, qrScanScreen, "BottomBar/BackToHub");

        experienceRouteText = FindText(experienceRouteText, arExperienceScreen, "TopBar/Route");
        experienceStatusText = FindText(experienceStatusText, arExperienceScreen, "BottomBar/Status");
        experienceQuestButton = FindButton(experienceQuestButton, arExperienceScreen, "BottomBar/QuestButton");
        experienceBackToHubButton = FindButton(experienceBackToHubButton, arExperienceScreen, "BottomBar/HubButton");
    }

    GameObject FindChild(GameObject current, string screenName)
    {
        if (current != null)
            return current;

        var found = transform.Find("Screens/" + screenName);
        if (found == null)
            found = transform.Find(screenName);

        return found != null ? found.gameObject : null;
    }

    static Transform FindTransform(Transform current, GameObject root, string path)
    {
        if (current != null)
            return current;

        if (root == null)
            return null;

        return root.transform.Find(path);
    }

    static TMP_Text FindText(TMP_Text current, GameObject root, string path)
    {
        if (current != null)
            return current;

        if (root == null)
            return null;

        var t = root.transform.Find(path);
        return t != null ? t.GetComponent<TMP_Text>() : null;
    }

    static Button FindButton(Button current, GameObject root, string path)
    {
        if (current != null)
            return current;

        if (root == null)
            return null;

        var t = root.transform.Find(path);
        return t != null ? t.GetComponent<Button>() : null;
    }
}

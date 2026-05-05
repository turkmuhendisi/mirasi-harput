using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IndoorNpcSetupUI : MonoBehaviour
{
    [SerializeField] IndoorNpcTestManager manager = null;
    [SerializeField] CanvasGroup panelCanvasGroup = null;
    [SerializeField] TMP_Text setupStatusText = null;
    [SerializeField] Button placeNpcButton = null;
    [SerializeField] Button resetSetupButton = null;
    [SerializeField] Button finishSetupButton = null;

    void Awake()
    {
        ResolveReferences();
        EnsureDefaultLayout();
        SubscribeButtons();
    }

    void OnEnable()
    {
        ResolveReferences();
        SubscribeButtons();
    }

    void OnDisable()
    {
        UnsubscribeButtons();
    }

    public void SetManager(IndoorNpcTestManager testManager)
    {
        manager = testManager;
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    public void SetStatus(string status)
    {
        if (setupStatusText != null)
            setupStatusText.text = string.IsNullOrEmpty(status) ? "-" : status;
    }

    public void SetFinishInteractable(bool value)
    {
        if (finishSetupButton != null)
            finishSetupButton.interactable = value;
    }

    void ResolveReferences()
    {
        if (manager == null)
            manager = FindAnyObjectByType<IndoorNpcTestManager>();

        if (panelCanvasGroup == null)
            panelCanvasGroup = GetComponent<CanvasGroup>();
    }

    void EnsureDefaultLayout()
    {
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0f, 24f);
            rectTransform.sizeDelta = new Vector2(560f, 178f);
        }

        var image = GetComponent<Image>();
        if (image == null)
            image = gameObject.AddComponent<Image>();

        image.color = new Color(0.03f, 0.04f, 0.05f, 0.84f);
        image.raycastTarget = true;

        if (panelCanvasGroup == null)
            panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (setupStatusText == null)
            setupStatusText = CreateText("SetupStatusText", "1. odada NPC-1'i yerleştirin.", new Vector2(16f, -14f), new Vector2(528f, 58f), 15f, FontStyles.Normal);

        if (placeNpcButton == null)
            placeNpcButton = CreateButton("PlaceNpcButton", "NPC Yerleştir", new Vector2(16f, 18f), new Vector2(160f, 42f), new Color(0.18f, 0.45f, 0.32f, 0.96f));

        if (resetSetupButton == null)
            resetSetupButton = CreateButton("ResetSetupButton", "Sıfırla", new Vector2(200f, 18f), new Vector2(130f, 42f), new Color(0.38f, 0.34f, 0.24f, 0.96f));

        if (finishSetupButton == null)
            finishSetupButton = CreateButton("FinishSetupButton", "Kurulumu Bitir", new Vector2(354f, 18f), new Vector2(190f, 42f), new Color(0.18f, 0.34f, 0.68f, 0.96f));
    }

    TMP_Text CreateText(string objectName, string value, Vector2 anchoredPosition, Vector2 size, float fontSize, FontStyles fontStyle)
    {
        var child = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        child.transform.SetParent(transform, false);

        var rect = child.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        var text = child.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    Button CreateButton(string objectName, string label, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        var child = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        child.transform.SetParent(transform, false);

        var rect = child.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        var image = child.GetComponent<Image>();
        image.color = color;

        var button = child.GetComponent<Button>();
        button.targetGraphic = image;

        var labelText = CreateText(objectName + "Text", label, Vector2.zero, new Vector2(size.x - 16f, size.y - 8f), 14f, FontStyles.Bold);
        labelText.transform.SetParent(child.transform, false);
        var labelRect = labelText.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = new Vector2(-12f, -4f);
        labelText.alignment = TextAlignmentOptions.Center;

        return button;
    }

    void SubscribeButtons()
    {
        AddButtonListener(placeNpcButton, HandlePlaceClicked);
        AddButtonListener(resetSetupButton, HandleResetClicked);
        AddButtonListener(finishSetupButton, HandleFinishClicked);
    }

    void UnsubscribeButtons()
    {
        RemoveButtonListener(placeNpcButton, HandlePlaceClicked);
        RemoveButtonListener(resetSetupButton, HandleResetClicked);
        RemoveButtonListener(finishSetupButton, HandleFinishClicked);
    }

    void HandlePlaceClicked()
    {
        if (manager != null)
            manager.StartSetupMode();
    }

    void HandleResetClicked()
    {
        if (manager != null)
            manager.ResetIndoorTest();
    }

    void HandleFinishClicked()
    {
        if (manager != null)
            manager.CompleteSetupMode();
    }

    void SetVisible(bool visible)
    {
        if (panelCanvasGroup == null)
            return;

        panelCanvasGroup.alpha = visible ? 1f : 0f;
        panelCanvasGroup.interactable = visible;
        panelCanvasGroup.blocksRaycasts = visible;
    }

    static void AddButtonListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    static void RemoveButtonListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
            button.onClick.RemoveListener(action);
    }
}

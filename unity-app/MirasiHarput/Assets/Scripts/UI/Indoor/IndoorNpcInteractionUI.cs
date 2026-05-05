using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IndoorNpcInteractionUI : MonoBehaviour
{
    [SerializeField] CanvasGroup panelCanvasGroup = null;
    [SerializeField] TMP_Text npcNameText = null;
    [SerializeField] TMP_Text dialogueText = null;
    [SerializeField] TMP_Text objectiveText = null;
    [SerializeField] Button continueButton = null;
    [SerializeField] GameObject resultPanel = null;
    [SerializeField] CanvasGroup resultCanvasGroup = null;
    [SerializeField] TMP_Text resultTitleText = null;
    [SerializeField] TMP_Text resultDescriptionText = null;
    [SerializeField] TMP_Text rewardText = null;
    [SerializeField] TMP_Text badgeText = null;
    [SerializeField] Button resultCloseButton = null;
    [SerializeField] Button closeButton = null;

    public bool IsVisible => panelCanvasGroup != null && panelCanvasGroup.alpha > 0.01f;

    void Awake()
    {
        EnsureDefaultLayout();
        SubscribeButtons();
        Hide();
    }

    void OnEnable()
    {
        SubscribeButtons();
    }

    void OnDisable()
    {
        UnsubscribeButtons();
    }

    public void ShowNpcDialogue(string npcName, string dialogue, string objective)
    {
        SetVisible(true);
        SetResultVisible(false);
        SetText(npcNameText, npcName);
        SetText(dialogueText, dialogue);
        SetText(objectiveText, objective);
    }

    public void ShowResult(int rewardScore, string badgeId, string resultDescription)
    {
        SetVisible(false);
        MoveResultPanelToOverlay();
        SetResultVisible(true);
        SetText(resultTitleText, "Indoor NPC test görevi tamamlandı");
        SetText(resultDescriptionText, resultDescription);
        SetText(rewardText, "Kazanılan Puan: " + Mathf.Max(0, rewardScore));
        SetText(badgeText, "Rozet: " + (string.IsNullOrEmpty(badgeId) ? "-" : badgeId));
    }

    public void Hide()
    {
        SetVisible(false);
        SetResultVisible(false);
    }

    void EnsureDefaultLayout()
    {
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0f, 220f);
            rectTransform.sizeDelta = new Vector2(620f, 246f);
        }

        var image = GetComponent<Image>();
        if (image == null)
            image = gameObject.AddComponent<Image>();

        image.color = new Color(0.04f, 0.05f, 0.06f, 0.86f);
        image.raycastTarget = true;

        if (panelCanvasGroup == null)
            panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (npcNameText == null)
            npcNameText = CreateText(transform, "NpcNameText", "Rehber", new Vector2(18f, -16f), new Vector2(420f, 28f), 18f, FontStyles.Bold);

        if (dialogueText == null)
            dialogueText = CreateText(transform, "DialogueText", "Diyalog", new Vector2(18f, -54f), new Vector2(584f, 76f), 15f, FontStyles.Normal);

        if (objectiveText == null)
            objectiveText = CreateText(transform, "ObjectiveText", "Hedef: -", new Vector2(18f, -138f), new Vector2(584f, 34f), 14f, FontStyles.Bold);

        if (continueButton == null)
            continueButton = CreateButton(transform, "ContinueButton", "Devam", new Vector2(18f, 18f), new Vector2(150f, 42f), new Color(0.18f, 0.42f, 0.32f, 0.96f));

        if (closeButton == null)
            closeButton = CreateButton(transform, "CloseButton", "Kapat", new Vector2(452f, 18f), new Vector2(150f, 42f), new Color(0.34f, 0.34f, 0.34f, 0.96f));

        if (resultPanel == null)
            CreateResultPanel();
    }

    void CreateResultPanel()
    {
        resultPanel = new GameObject("ResultPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        resultPanel.transform.SetParent(GetOverlayParent(), false);

        var rect = resultPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 0f);
        rect.sizeDelta = new Vector2(480f, 220f);

        var image = resultPanel.GetComponent<Image>();
        image.color = new Color(0.07f, 0.09f, 0.11f, 0.96f);

        resultCanvasGroup = resultPanel.GetComponent<CanvasGroup>();
        resultTitleText = CreateText(resultPanel.transform, "ResultTitleText", "Indoor NPC test görevi tamamlandı", new Vector2(20f, -18f), new Vector2(440f, 30f), 18f, FontStyles.Bold);
        resultDescriptionText = CreateText(resultPanel.transform, "ResultDescriptionText", "Görev sonucu", new Vector2(20f, -56f), new Vector2(440f, 50f), 14f, FontStyles.Normal);
        rewardText = CreateText(resultPanel.transform, "RewardText", "Kazanılan Puan: 20", new Vector2(20f, -112f), new Vector2(440f, 24f), 14f, FontStyles.Bold);
        badgeText = CreateText(resultPanel.transform, "BadgeText", "Rozet: badge_indoor_npc_test", new Vector2(20f, -140f), new Vector2(440f, 24f), 14f, FontStyles.Bold);
        resultCloseButton = CreateButton(resultPanel.transform, "ResultCloseButton", "Tamam", new Vector2(165f, 18f), new Vector2(150f, 42f), new Color(0.18f, 0.42f, 0.32f, 0.96f));
    }

    Transform GetOverlayParent()
    {
        return transform.parent != null ? transform.parent : transform;
    }

    void MoveResultPanelToOverlay()
    {
        if (resultPanel == null)
            return;

        resultPanel.transform.SetParent(GetOverlayParent(), false);
        resultPanel.transform.SetAsLastSibling();

        var rect = resultPanel.GetComponent<RectTransform>();
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(480f, 220f);
    }

    TMP_Text CreateText(Transform parent, string objectName, string value, Vector2 anchoredPosition, Vector2 size, float fontSize, FontStyles fontStyle)
    {
        var child = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        child.transform.SetParent(parent, false);

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

    Button CreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        var child = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        child.transform.SetParent(parent, false);

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

        var labelText = CreateText(child.transform, objectName + "Text", label, Vector2.zero, size, 14f, FontStyles.Bold);
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
        AddButtonListener(continueButton, HandleContinueClicked);
        AddButtonListener(closeButton, HandleCloseClicked);
        AddButtonListener(resultCloseButton, HandleCloseClicked);
    }

    void UnsubscribeButtons()
    {
        RemoveButtonListener(continueButton, HandleContinueClicked);
        RemoveButtonListener(closeButton, HandleCloseClicked);
        RemoveButtonListener(resultCloseButton, HandleCloseClicked);
    }

    void HandleContinueClicked()
    {
        if (resultCanvasGroup != null && resultCanvasGroup.alpha > 0.5f)
            Hide();
        else
            SetVisible(false);
    }

    void HandleCloseClicked()
    {
        Hide();
    }

    void SetVisible(bool visible)
    {
        if (panelCanvasGroup == null)
            return;

        panelCanvasGroup.alpha = visible ? 1f : 0f;
        panelCanvasGroup.interactable = visible;
        panelCanvasGroup.blocksRaycasts = visible;
    }

    void SetResultVisible(bool visible)
    {
        if (resultCanvasGroup == null)
            return;

        resultCanvasGroup.alpha = visible ? 1f : 0f;
        resultCanvasGroup.interactable = visible;
        resultCanvasGroup.blocksRaycasts = visible;
    }

    static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
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

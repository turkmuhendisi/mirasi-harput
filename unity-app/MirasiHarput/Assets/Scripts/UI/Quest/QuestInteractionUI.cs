using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestInteractionUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] QuestProgressManager questProgressManager = null;
    [SerializeField] LocationTriggerManager locationTriggerManager = null;
    [SerializeField] JsonDataLoader dataLoader = null;

    [Header("Panel")]
    [SerializeField] CanvasGroup panelCanvasGroup = null;
    [SerializeField] bool hideOnStart = true;

    [Header("Top")]
    [SerializeField] TMP_Text locationNameText = null;
    [SerializeField] TMP_Text questTitleText = null;
    [SerializeField] TMP_Text questStateText = null;

    [Header("NPC Dialogue")]
    [SerializeField] TMP_Text npcDialogueText = null;
    [SerializeField] Button continueDialogueButton = null;

    [Header("Quest Content")]
    [SerializeField] TMP_Text questDescriptionText = null;
    [SerializeField] TMP_Text objectiveText = null;

    [Header("Question")]
    [SerializeField] GameObject questionSectionRoot = null;
    [SerializeField] TMP_Text questionText = null;
    [SerializeField] Transform answerOptionsContainer = null;
    [SerializeField] AnswerOptionButton answerOptionButtonPrefab = null;
    [SerializeField] TMP_Text feedbackText = null;

    [Header("Actions")]
    [SerializeField] Button startQuestButton = null;
    [SerializeField] Button completeQuestButton = null;
    [SerializeField] Button closePanelButton = null;

    [Header("Result")]
    [SerializeField] QuestResultUI resultUI = null;

    readonly List<AnswerOptionButton> answerButtons = new List<AnswerOptionButton>();

    QuestData currentQuest;
    LocationData currentLocation;
    bool hasQuestion;
    bool hasCorrectAnswer;
    bool dialogueAdvanced;

    void Awake()
    {
        ResolveReferences();
        SubscribeButtons();

        if (answerOptionButtonPrefab != null)
            answerOptionButtonPrefab.gameObject.SetActive(false);

        if (hideOnStart)
            HidePanel();
    }

    void OnEnable()
    {
        ResolveReferences();
        SubscribeGameplayEvents();
        SubscribeButtons();

        if (resultUI != null)
            resultUI.OnContinueRoute += HandleContinueRoute;
    }

    void Start()
    {
        ResolveReferences();
        SubscribeGameplayEvents();
        TryShowCurrentQuest();
    }

    void OnDisable()
    {
        UnsubscribeGameplayEvents();
        UnsubscribeButtons();

        if (resultUI != null)
            resultUI.OnContinueRoute -= HandleContinueRoute;
    }

    void ResolveReferences()
    {
        if (panelCanvasGroup == null)
            panelCanvasGroup = GetComponent<CanvasGroup>();

        if (questProgressManager == null)
            questProgressManager = FindAnyObjectByType<QuestProgressManager>();

        if (locationTriggerManager == null)
            locationTriggerManager = FindAnyObjectByType<LocationTriggerManager>();

        if (dataLoader == null)
            dataLoader = JsonDataLoader.Instance != null ? JsonDataLoader.Instance : FindAnyObjectByType<JsonDataLoader>();

        if (resultUI == null)
            resultUI = GetComponentInChildren<QuestResultUI>(true);
    }

    void SubscribeGameplayEvents()
    {
        if (questProgressManager != null)
        {
            questProgressManager.OnQuestAvailable -= HandleQuestAvailable;
            questProgressManager.OnQuestAvailable += HandleQuestAvailable;
            questProgressManager.OnQuestStarted -= HandleQuestStarted;
            questProgressManager.OnQuestStarted += HandleQuestStarted;
            questProgressManager.OnQuestCompleted -= HandleQuestCompleted;
            questProgressManager.OnQuestCompleted += HandleQuestCompleted;
        }

        if (locationTriggerManager != null)
        {
            locationTriggerManager.OnTargetLocationChanged -= HandleTargetLocationChanged;
            locationTriggerManager.OnTargetLocationChanged += HandleTargetLocationChanged;
        }
    }

    void UnsubscribeGameplayEvents()
    {
        if (questProgressManager != null)
        {
            questProgressManager.OnQuestAvailable -= HandleQuestAvailable;
            questProgressManager.OnQuestStarted -= HandleQuestStarted;
            questProgressManager.OnQuestCompleted -= HandleQuestCompleted;
        }

        if (locationTriggerManager != null)
            locationTriggerManager.OnTargetLocationChanged -= HandleTargetLocationChanged;
    }

    void SubscribeButtons()
    {
        AddButtonListener(startQuestButton, HandleStartQuestClicked);
        AddButtonListener(completeQuestButton, HandleCompleteQuestClicked);
        AddButtonListener(closePanelButton, HandleCloseClicked);
        AddButtonListener(continueDialogueButton, HandleContinueDialogueClicked);
    }

    void UnsubscribeButtons()
    {
        RemoveButtonListener(startQuestButton, HandleStartQuestClicked);
        RemoveButtonListener(completeQuestButton, HandleCompleteQuestClicked);
        RemoveButtonListener(closePanelButton, HandleCloseClicked);
        RemoveButtonListener(continueDialogueButton, HandleContinueDialogueClicked);
    }

    void TryShowCurrentQuest()
    {
        if (questProgressManager == null || questProgressManager.ActiveQuest == null)
            return;

        var state = questProgressManager.ActiveQuestState;
        if (state == QuestState.Available || state == QuestState.Started)
            ShowQuest(questProgressManager.ActiveQuest);
    }

    void HandleQuestAvailable(QuestData quest)
    {
        ShowQuest(quest);
    }

    void HandleQuestStarted(QuestData quest)
    {
        ShowQuest(quest);
    }

    void HandleQuestCompleted(QuestData quest, int rewardPoint, string badgeId)
    {
        if (quest == null)
            return;

        currentQuest = quest;
        currentLocation = FindLocationForQuest(quest);
        ShowPanel();
        RenderQuest();

        var nextLocation = FindNextLocationForQuest(quest);
        if (resultUI != null)
            resultUI.ShowResult(quest, rewardPoint, badgeId, nextLocation);

        SetText(feedbackText, string.IsNullOrEmpty(quest.npcDialogueComplete) ? "Görev tamamlandı." : quest.npcDialogueComplete);
        RefreshActionButtons();
    }

    void HandleTargetLocationChanged(LocationData location)
    {
        if (resultUI != null && resultUI.IsVisible && currentQuest != null)
            resultUI.ShowResult(currentQuest, currentQuest.rewardPoint, currentQuest.badgeId, location);
    }

    void ShowQuest(QuestData quest)
    {
        if (quest == null)
            return;

        currentQuest = quest;
        currentLocation = FindLocationForQuest(quest);
        hasCorrectAnswer = false;
        dialogueAdvanced = false;

        ShowPanel();
        RenderQuest();

        if (resultUI != null)
            resultUI.HideResult();
    }

    void RenderQuest()
    {
        if (currentQuest == null)
            return;

        var state = questProgressManager != null ? questProgressManager.GetQuestState(currentQuest.id) : QuestState.Available;
        SetText(locationNameText, currentLocation != null ? currentLocation.name : "-");
        SetText(questTitleText, currentQuest.title);
        SetText(questStateText, "Durum: " + state);
        SetText(npcDialogueText, string.IsNullOrEmpty(currentQuest.npcDialogueStart) ? "-" : currentQuest.npcDialogueStart);
        SetText(questDescriptionText, currentQuest.description);
        SetText(objectiveText, "Hedef: " + currentQuest.objectiveText);

        BuildQuestionArea();
        RefreshActionButtons();
    }

    void BuildQuestionArea()
    {
        ClearAnswerOptions();

        hasQuestion = currentQuest != null && !string.IsNullOrWhiteSpace(currentQuest.questionText);
        if (questionSectionRoot != null)
            questionSectionRoot.SetActive(hasQuestion);

        if (!hasQuestion)
        {
            hasCorrectAnswer = true;
            SetText(questionText, string.Empty);
            SetText(feedbackText, "Bu görevde soru yok. Görevi tamamlayabilirsin.");
            return;
        }

        hasCorrectAnswer = false;
        SetText(questionText, currentQuest.questionText);
        SetText(feedbackText, "Cevabını seç.");

        var answerOptions = CreateShuffledAnswers(currentQuest);
        for (var i = 0; i < answerOptions.Count; i++)
            CreateAnswerButton(answerOptions[i].Text, answerOptions[i].IsCorrect);
    }

    List<AnswerOption> CreateShuffledAnswers(QuestData quest)
    {
        var answers = new List<AnswerOption>();

        if (!string.IsNullOrEmpty(quest.correctAnswer))
            answers.Add(new AnswerOption(quest.correctAnswer, true));

        if (quest.wrongAnswers != null)
        {
            for (var i = 0; i < quest.wrongAnswers.Length; i++)
            {
                var wrongAnswer = quest.wrongAnswers[i];
                if (!string.IsNullOrEmpty(wrongAnswer))
                    answers.Add(new AnswerOption(wrongAnswer, false));
            }
        }

        for (var i = answers.Count - 1; i > 0; i--)
        {
            var swapIndex = Random.Range(0, i + 1);
            var temp = answers[i];
            answers[i] = answers[swapIndex];
            answers[swapIndex] = temp;
        }

        return answers;
    }

    void CreateAnswerButton(string answerTextValue, bool isCorrect)
    {
        if (answerOptionButtonPrefab == null || answerOptionsContainer == null)
            return;

        var answerButton = Instantiate(answerOptionButtonPrefab, answerOptionsContainer);
        var buttonRect = answerButton.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.anchorMin = new Vector2(0f, 1f);
            buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.pivot = new Vector2(0.5f, 1f);
            buttonRect.anchoredPosition = new Vector2(0f, -answerButtons.Count * 42f);
            buttonRect.sizeDelta = new Vector2(0f, 36f);
        }

        answerButton.gameObject.SetActive(true);
        answerButton.Setup(answerTextValue, isCorrect, HandleAnswerSelected);
        answerButtons.Add(answerButton);
    }

    void ClearAnswerOptions()
    {
        for (var i = 0; i < answerButtons.Count; i++)
        {
            if (answerButtons[i] != null)
                Destroy(answerButtons[i].gameObject);
        }

        answerButtons.Clear();

        if (answerOptionButtonPrefab != null)
            answerOptionButtonPrefab.gameObject.SetActive(false);
    }

    void HandleAnswerSelected(bool isCorrect, string selectedAnswer)
    {
        hasCorrectAnswer = isCorrect;

        if (isCorrect)
        {
            SetText(feedbackText, "Doğru cevap! Görevi tamamlayabilirsin.");
            SetAnswerButtonsInteractable(false);
        }
        else
        {
            SetText(feedbackText, "Yanlış cevap, tekrar dene.");
        }

        RefreshActionButtons();
    }

    void SetAnswerButtonsInteractable(bool interactable)
    {
        for (var i = 0; i < answerButtons.Count; i++)
        {
            if (answerButtons[i] != null)
                answerButtons[i].SetInteractable(interactable);
        }
    }

    void HandleStartQuestClicked()
    {
        if (questProgressManager != null && currentQuest != null)
            questProgressManager.StartQuest(currentQuest.id);

        RefreshActionButtons();
    }

    void HandleCompleteQuestClicked()
    {
        if (questProgressManager != null && currentQuest != null)
            questProgressManager.CompleteQuest(currentQuest.id);

        RefreshActionButtons();
    }

    void HandleCloseClicked()
    {
        HidePanel();
    }

    void HandleContinueDialogueClicked()
    {
        if (currentQuest == null)
            return;

        dialogueAdvanced = !dialogueAdvanced;
        SetText(npcDialogueText, dialogueAdvanced ? currentQuest.objectiveText : currentQuest.npcDialogueStart);
    }

    void HandleContinueRoute()
    {
        HidePanel();
    }

    void RefreshActionButtons()
    {
        var hasQuest = currentQuest != null;
        var state = hasQuest && questProgressManager != null ? questProgressManager.GetQuestState(currentQuest.id) : QuestState.Locked;
        var canStart = hasQuest && state == QuestState.Available;
        var canComplete = hasQuest && state == QuestState.Started && (!hasQuestion || hasCorrectAnswer);

        SetButtonInteractable(startQuestButton, canStart);
        SetButtonInteractable(completeQuestButton, canComplete);
    }

    LocationData FindLocationForQuest(QuestData quest)
    {
        if (quest == null)
            return null;

        if (questProgressManager != null && questProgressManager.ActiveLocation != null &&
            questProgressManager.ActiveLocation.id == quest.locationId)
        {
            return questProgressManager.ActiveLocation;
        }

        if (dataLoader != null)
            return dataLoader.GetLocationById(quest.locationId);

        return null;
    }

    LocationData FindNextLocationForQuest(QuestData quest)
    {
        if (quest == null || dataLoader == null)
            return null;

        if (!string.IsNullOrEmpty(quest.nextLocationId))
            return dataLoader.GetLocationById(quest.nextLocationId);

        return dataLoader.GetNextLocation(quest.locationId);
    }

    void ShowPanel()
    {
        if (panelCanvasGroup == null)
            return;

        panelCanvasGroup.alpha = 1f;
        panelCanvasGroup.interactable = true;
        panelCanvasGroup.blocksRaycasts = true;
    }

    public void HidePanel()
    {
        if (panelCanvasGroup == null)
            return;

        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;

        if (resultUI != null)
            resultUI.HideResult();
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

    static void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    readonly struct AnswerOption
    {
        public readonly string Text;
        public readonly bool IsCorrect;

        public AnswerOption(string text, bool isCorrect)
        {
            Text = text;
            IsCorrect = isCorrect;
        }
    }
}

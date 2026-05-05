using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestProgressDebugUI : MonoBehaviour
{
    [SerializeField] QuestProgressManager questProgressManager = null;

    [Header("Text References")]
    [SerializeField] TMP_Text questStatusText = null;
    [SerializeField] TMP_Text activeQuestTitleText = null;
    [SerializeField] TMP_Text activeQuestStateText = null;
    [SerializeField] TMP_Text activeLocationText = null;
    [SerializeField] TMP_Text totalScoreText = null;
    [SerializeField] TMP_Text completedQuestCountText = null;
    [SerializeField] TMP_Text lastCompletedQuestText = null;
    [SerializeField] TMP_Text lastBadgeText = null;
    [SerializeField] TMP_Text progressText = null;

    [Header("Buttons")]
    [SerializeField] Button startQuestButton = null;
    [SerializeField] Button completeQuestButton = null;
    [SerializeField] Button resetProgressButton = null;

    [SerializeField, Min(0.05f)] float refreshInterval = 0.25f;

    float nextRefreshTime;
    bool buttonsSubscribed;

    void OnEnable()
    {
        ResolveManager();
        SubscribeButtons();
        Refresh();
    }

    void Start()
    {
        ResolveManager();
        SubscribeButtons();
        Refresh();
    }

    void Update()
    {
        if (Time.unscaledTime < nextRefreshTime)
            return;

        nextRefreshTime = Time.unscaledTime + refreshInterval;
        Refresh();
    }

    void OnDisable()
    {
        UnsubscribeButtons();
    }

    void ResolveManager()
    {
        if (questProgressManager != null)
            return;

        questProgressManager = FindAnyObjectByType<QuestProgressManager>();
    }

    void SubscribeButtons()
    {
        if (buttonsSubscribed)
            return;

        if (startQuestButton != null)
            startQuestButton.onClick.AddListener(StartActiveQuest);

        if (completeQuestButton != null)
            completeQuestButton.onClick.AddListener(CompleteActiveQuest);

        if (resetProgressButton != null)
            resetProgressButton.onClick.AddListener(ResetProgress);

        buttonsSubscribed = true;
    }

    void UnsubscribeButtons()
    {
        if (!buttonsSubscribed)
            return;

        if (startQuestButton != null)
            startQuestButton.onClick.RemoveListener(StartActiveQuest);

        if (completeQuestButton != null)
            completeQuestButton.onClick.RemoveListener(CompleteActiveQuest);

        if (resetProgressButton != null)
            resetProgressButton.onClick.RemoveListener(ResetProgress);

        buttonsSubscribed = false;
    }

    public void StartActiveQuest()
    {
        ResolveManager();
        if (questProgressManager != null && questProgressManager.ActiveQuest != null)
            questProgressManager.StartQuest(questProgressManager.ActiveQuest.id);

        Refresh();
    }

    public void CompleteActiveQuest()
    {
        ResolveManager();
        if (questProgressManager != null)
            questProgressManager.CompleteActiveQuest();

        Refresh();
    }

    public void ResetProgress()
    {
        ResolveManager();
        if (questProgressManager != null)
            questProgressManager.ResetQuestProgress();

        Refresh();
    }

    void Refresh()
    {
        ResolveManager();

        if (questProgressManager == null)
        {
            SetText(questStatusText, "Quest status: QuestProgressManager bulunamadı");
            SetText(activeQuestTitleText, "Active quest: -");
            SetText(activeQuestStateText, "Quest state: -");
            SetText(activeLocationText, "Active location: -");
            SetText(totalScoreText, "Total score: -");
            SetText(completedQuestCountText, "Completed quests: -");
            SetText(lastCompletedQuestText, "Last completed: -");
            SetText(lastBadgeText, "Last badge: -");
            SetText(progressText, "Progress: -");
            SetButtonsInteractable(false, false, false);
            return;
        }

        var activeQuest = questProgressManager.ActiveQuest;
        var activeLocation = questProgressManager.ActiveLocation;
        var activeState = questProgressManager.ActiveQuestState;
        var hasActiveQuest = activeQuest != null;

        SetText(questStatusText, "Quest status: " + questProgressManager.StatusMessage);
        SetText(activeQuestTitleText, "Active quest: " + (hasActiveQuest ? activeQuest.title : "-"));
        SetText(activeQuestStateText, "Quest state: " + (hasActiveQuest ? activeState.ToString() : "-"));
        SetText(activeLocationText, "Active location: " + (activeLocation != null ? activeLocation.name : "-"));
        SetText(totalScoreText, "Total score: " + questProgressManager.TotalScore);
        SetText(completedQuestCountText, "Completed quests: " + questProgressManager.CompletedQuestCount);
        SetText(lastCompletedQuestText, "Last completed: " + FormatEmpty(questProgressManager.LastCompletedQuestId));
        SetText(lastBadgeText, "Last badge: " + FormatEmpty(questProgressManager.LastUnlockedBadgeId));
        SetText(progressText, "Progress: " + questProgressManager.GetProgressText());

        var canStart = hasActiveQuest && activeState != QuestState.Started && activeState != QuestState.Completed;
        var canComplete = hasActiveQuest && activeState == QuestState.Started;
        SetButtonsInteractable(canStart, canComplete, true);
    }

    void SetButtonsInteractable(bool canStart, bool canComplete, bool canReset)
    {
        if (startQuestButton != null)
            startQuestButton.interactable = canStart;

        if (completeQuestButton != null)
            completeQuestButton.interactable = canComplete;

        if (resetProgressButton != null)
            resetProgressButton.interactable = canReset;
    }

    static string FormatEmpty(string value)
    {
        return string.IsNullOrEmpty(value) ? "-" : value;
    }

    static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}

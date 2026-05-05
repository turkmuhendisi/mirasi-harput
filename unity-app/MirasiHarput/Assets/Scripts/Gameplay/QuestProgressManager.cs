using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestProgressManager : MonoBehaviour
{
    [SerializeField] LocationTriggerManager locationTriggerManager = null;
    [SerializeField] JsonDataLoader dataLoader = null;

    [Header("Quest Flow")]
    [SerializeField] bool autoStartQuestOnLocationTrigger = true;
    [SerializeField] bool autoAdvanceRouteOnQuestComplete = true;
    [SerializeField] bool preventCompletingWithoutStart = true;
    [SerializeField] bool allowQuestRestart = false;

    readonly Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();
    readonly HashSet<string> completedQuestIds = new HashSet<string>();
    readonly HashSet<string> unlockedBadgeIds = new HashSet<string>();

    bool isSubscribedToDataLoader;
    bool isSubscribedToTriggerManager;

    public event Action<QuestData> OnQuestAvailable;
    public event Action<QuestData> OnQuestStarted;
    public event Action<QuestData, int, string> OnQuestCompleted;
    public event Action<int> OnScoreChanged;
    public event Action<string> OnBadgeUnlocked;

    public QuestData ActiveQuest { get; private set; }
    public LocationData ActiveLocation { get; private set; }
    public QuestState ActiveQuestState { get; private set; } = QuestState.Locked;
    public int TotalScore { get; private set; }
    public int CompletedQuestCount
    {
        get { return completedQuestIds.Count; }
    }

    public string LastCompletedQuestId { get; private set; } = string.Empty;
    public string LastUnlockedBadgeId { get; private set; } = string.Empty;
    public string StatusMessage { get; private set; } = "Quest progress bekleniyor";

    void OnEnable()
    {
        ResolveReferences();
        SubscribeEvents();
    }

    void Start()
    {
        ResolveReferences();
        SubscribeEvents();
        InitializeQuestStates();
        SyncActiveTriggerQuest();
    }

    void OnDestroy()
    {
        UnsubscribeEvents();
    }

    void ResolveReferences()
    {
        if (locationTriggerManager == null)
            locationTriggerManager = FindAnyObjectByType<LocationTriggerManager>();

        if (dataLoader == null)
            dataLoader = JsonDataLoader.Instance != null ? JsonDataLoader.Instance : FindAnyObjectByType<JsonDataLoader>();
    }

    void SubscribeEvents()
    {
        if (dataLoader != null && !isSubscribedToDataLoader)
        {
            dataLoader.OnDataLoaded += HandleDataLoaded;
            isSubscribedToDataLoader = true;
        }

        if (locationTriggerManager != null && !isSubscribedToTriggerManager)
        {
            locationTriggerManager.OnLocationTriggered += HandleLocationTriggered;
            isSubscribedToTriggerManager = true;
        }
    }

    void UnsubscribeEvents()
    {
        if (dataLoader != null && isSubscribedToDataLoader)
        {
            dataLoader.OnDataLoaded -= HandleDataLoaded;
            isSubscribedToDataLoader = false;
        }

        if (locationTriggerManager != null && isSubscribedToTriggerManager)
        {
            locationTriggerManager.OnLocationTriggered -= HandleLocationTriggered;
            isSubscribedToTriggerManager = false;
        }
    }

    void HandleDataLoaded(JsonDataLoader loadedData)
    {
        if (loadedData != null)
            dataLoader = loadedData;

        InitializeQuestStates();
        SyncActiveTriggerQuest();
    }

    void SyncActiveTriggerQuest()
    {
        if (locationTriggerManager == null || locationTriggerManager.ActiveQuest == null)
            return;

        HandleLocationTriggered(locationTriggerManager.ActiveLocation, locationTriggerManager.ActiveQuest);
    }

    public void InitializeQuestStates()
    {
        ResolveReferences();
        SubscribeEvents();

        if (dataLoader == null)
        {
            StatusMessage = "JsonDataLoader bulunamadı";
            return;
        }

        if (!dataLoader.IsLoaded)
        {
            StatusMessage = "Görev datası bekleniyor";
            return;
        }

        questStates.Clear();

        for (var i = 0; i < dataLoader.Quests.Count; i++)
        {
            var quest = dataLoader.Quests[i];
            if (quest == null || string.IsNullOrEmpty(quest.id))
                continue;

            questStates[quest.id] = completedQuestIds.Contains(quest.id) ? QuestState.Completed : QuestState.Locked;
        }

        var startLocation = dataLoader.GetStartLocation();
        var startQuest = startLocation != null ? dataLoader.GetQuestByLocationId(startLocation.id) : null;
        if (startQuest != null)
            SetQuestAvailable(startQuest.id);

        ActiveQuestState = ActiveQuest != null ? GetQuestState(ActiveQuest.id) : QuestState.Locked;
        StatusMessage = "Görev durumları hazır";
    }

    public void HandleLocationTriggered(LocationData location, QuestData quest)
    {
        ResolveReferences();

        if (quest == null && location != null && dataLoader != null)
            quest = dataLoader.GetQuestByLocationId(location.id);

        ActiveLocation = location;

        if (quest == null || string.IsNullOrEmpty(quest.id))
        {
            ActiveQuest = null;
            ActiveQuestState = QuestState.Locked;
            StatusMessage = "Tetiklenen lokasyon için görev yok";
            return;
        }

        ActiveQuest = quest;
        SetQuestAvailable(quest.id);

        if (autoStartQuestOnLocationTrigger)
            StartQuest(quest.id);
    }

    public bool SetQuestAvailable(string questId)
    {
        if (string.IsNullOrEmpty(questId))
        {
            StatusMessage = "Görev id boş";
            return false;
        }

        var quest = GetQuest(questId);
        if (quest == null)
        {
            StatusMessage = "Görev bulunamadı: " + questId;
            return false;
        }

        var currentState = GetQuestState(questId);
        if (currentState == QuestState.Completed && !allowQuestRestart)
        {
            UpdateActiveQuestStateIfNeeded(questId);
            StatusMessage = "Görev zaten tamamlandı: " + quest.title;
            return false;
        }

        if (currentState != QuestState.Available && currentState != QuestState.Started)
        {
            questStates[questId] = QuestState.Available;
            OnQuestAvailable?.Invoke(quest);
        }

        UpdateActiveQuestStateIfNeeded(questId);
        StatusMessage = "Görev erişilebilir: " + quest.title;
        return true;
    }

    public bool StartQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId))
        {
            StatusMessage = "Başlatılacak görev id boş";
            return false;
        }

        var quest = GetQuest(questId);
        if (quest == null)
        {
            StatusMessage = "Başlatılacak görev bulunamadı: " + questId;
            return false;
        }

        var currentState = GetQuestState(questId);
        if (currentState == QuestState.Completed && !allowQuestRestart)
        {
            UpdateActiveQuestStateIfNeeded(questId);
            StatusMessage = "Tamamlanan görev yeniden başlatılamaz: " + quest.title;
            return false;
        }

        if (currentState == QuestState.Locked)
            SetQuestAvailable(questId);

        questStates[questId] = QuestState.Started;
        if (ActiveQuest == null || ActiveQuest.id == questId)
            ActiveQuest = quest;

        UpdateActiveQuestStateIfNeeded(questId);
        StatusMessage = "Görev başlatıldı: " + quest.title;
        OnQuestStarted?.Invoke(quest);
        return true;
    }

    public bool CompleteActiveQuest()
    {
        if (ActiveQuest == null)
        {
            StatusMessage = "Aktif görev yok";
            return false;
        }

        return CompleteQuest(ActiveQuest.id);
    }

    public bool CompleteQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId))
        {
            StatusMessage = "Tamamlanacak görev id boş";
            return false;
        }

        var quest = GetQuest(questId);
        if (quest == null)
        {
            StatusMessage = "Tamamlanacak görev bulunamadı: " + questId;
            return false;
        }

        if (completedQuestIds.Contains(questId))
        {
            questStates[questId] = QuestState.Completed;
            UpdateActiveQuestStateIfNeeded(questId);
            StatusMessage = "Görev zaten tamamlandı: " + quest.title;
            return false;
        }

        var currentState = GetQuestState(questId);
        if (preventCompletingWithoutStart && currentState != QuestState.Started)
        {
            StatusMessage = "Görev başlamadan tamamlanamaz: " + quest.title;
            return false;
        }

        questStates[questId] = QuestState.Completed;
        completedQuestIds.Add(questId);
        LastCompletedQuestId = questId;

        TotalScore += Mathf.Max(0, quest.rewardPoint);
        OnScoreChanged?.Invoke(TotalScore);

        var badgeId = string.IsNullOrEmpty(quest.badgeId) ? string.Empty : quest.badgeId;
        if (!string.IsNullOrEmpty(badgeId) && unlockedBadgeIds.Add(badgeId))
        {
            LastUnlockedBadgeId = badgeId;
            OnBadgeUnlocked?.Invoke(badgeId);
        }

        if (ActiveQuest == null || ActiveQuest.id == questId)
            ActiveQuest = quest;

        UpdateActiveQuestStateIfNeeded(questId);
        StatusMessage = "Görev tamamlandı: " + quest.title;
        OnQuestCompleted?.Invoke(quest, quest.rewardPoint, badgeId);

        if (autoAdvanceRouteOnQuestComplete && locationTriggerManager != null)
            locationTriggerManager.AdvanceToNextRouteLocation();

        return true;
    }

    public void ResetQuestProgress()
    {
        completedQuestIds.Clear();
        unlockedBadgeIds.Clear();
        questStates.Clear();
        ActiveQuest = null;
        ActiveLocation = null;
        ActiveQuestState = QuestState.Locked;
        TotalScore = 0;
        LastCompletedQuestId = string.Empty;
        LastUnlockedBadgeId = string.Empty;
        StatusMessage = "Görev ilerlemesi sıfırlandı";
        OnScoreChanged?.Invoke(TotalScore);
        InitializeQuestStates();
    }

    public QuestState GetQuestState(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return QuestState.Locked;

        if (questStates.TryGetValue(questId, out var state))
            return state;

        return QuestState.Locked;
    }

    public bool IsQuestCompleted(string questId)
    {
        return !string.IsNullOrEmpty(questId) && completedQuestIds.Contains(questId);
    }

    public bool IsBadgeUnlocked(string badgeId)
    {
        return !string.IsNullOrEmpty(badgeId) && unlockedBadgeIds.Contains(badgeId);
    }

    public string[] GetCompletedQuestIds()
    {
        var values = new string[completedQuestIds.Count];
        completedQuestIds.CopyTo(values);
        return values;
    }

    public string[] GetUnlockedBadgeIds()
    {
        var values = new string[unlockedBadgeIds.Count];
        unlockedBadgeIds.CopyTo(values);
        return values;
    }

    public string GetProgressText()
    {
        var totalQuestCount = dataLoader != null ? dataLoader.Quests.Count : questStates.Count;
        return CompletedQuestCount + " / " + totalQuestCount + " görev tamamlandı";
    }

    QuestData GetQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return null;

        ResolveReferences();
        if (dataLoader == null)
            return null;

        return dataLoader.GetQuestById(questId);
    }

    void UpdateActiveQuestStateIfNeeded(string questId)
    {
        if (ActiveQuest != null && ActiveQuest.id == questId)
            ActiveQuestState = GetQuestState(questId);
    }
}

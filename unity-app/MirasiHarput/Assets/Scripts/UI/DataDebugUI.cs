using TMPro;
using UnityEngine;

public class DataDebugUI : MonoBehaviour
{
    [SerializeField] JsonDataLoader dataLoader = null;

    [Header("Text References")]
    [SerializeField] TMP_Text statusText = null;
    [SerializeField] TMP_Text locationCountText = null;
    [SerializeField] TMP_Text questCountText = null;
    [SerializeField] TMP_Text routeNameText = null;
    [SerializeField] TMP_Text startLocationText = null;
    [SerializeField] TMP_Text firstQuestText = null;
    [SerializeField] TMP_Text errorText = null;

    [SerializeField, Min(0.05f)] float refreshInterval = 0.25f;

    bool subscribedToDataLoaded;
    float nextRefreshTime;

    void OnEnable()
    {
        ResolveDataLoader();
        SubscribeToDataLoaded();
        Refresh();
    }

    void Start()
    {
        ResolveDataLoader();
        SubscribeToDataLoaded();
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
        if (subscribedToDataLoaded && dataLoader != null)
            dataLoader.OnDataLoaded -= HandleDataLoaded;

        subscribedToDataLoaded = false;
    }

    void ResolveDataLoader()
    {
        if (dataLoader != null)
            return;

        dataLoader = JsonDataLoader.Instance;
        if (dataLoader == null)
            dataLoader = FindAnyObjectByType<JsonDataLoader>();
    }

    void SubscribeToDataLoaded()
    {
        if (subscribedToDataLoaded || dataLoader == null)
            return;

        dataLoader.OnDataLoaded += HandleDataLoaded;
        subscribedToDataLoaded = true;
    }

    void HandleDataLoaded(JsonDataLoader loadedData)
    {
        Refresh();
    }

    void Refresh()
    {
        ResolveDataLoader();
        SubscribeToDataLoaded();

        if (dataLoader == null)
        {
            SetText(statusText, "Data load status: JsonDataLoader bulunamadı");
            SetText(locationCountText, "Locations: -");
            SetText(questCountText, "Quests: -");
            SetText(routeNameText, "Route: -");
            SetText(startLocationText, "Start: -");
            SetText(firstQuestText, "First quest: -");
            SetText(errorText, "Error: JsonDataLoader sahnede yok");
            return;
        }

        SetText(statusText, "Data load status: " + dataLoader.StatusMessage);
        SetText(locationCountText, "Locations: " + dataLoader.Locations.Count);
        SetText(questCountText, "Quests: " + dataLoader.Quests.Count);

        var route = dataLoader.GetPrimaryRoute();
        SetText(routeNameText, "Route: " + (route != null ? route.routeName : "-"));

        var startLocation = dataLoader.GetStartLocation();
        SetText(startLocationText, "Start: " + (startLocation != null ? startLocation.name : "-"));

        var firstQuest = dataLoader.Quests.Count > 0 ? dataLoader.Quests[0] : null;
        SetText(firstQuestText, "First quest: " + (firstQuest != null ? firstQuest.title : "-"));

        var error = string.IsNullOrEmpty(dataLoader.ErrorMessage) ? "-" : dataLoader.ErrorMessage;
        SetText(errorText, "Error: " + error);
    }

    static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}

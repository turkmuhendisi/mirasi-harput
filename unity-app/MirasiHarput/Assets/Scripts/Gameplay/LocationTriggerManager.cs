using System;
using System.Collections.Generic;
using UnityEngine;

public class LocationTriggerManager : MonoBehaviour
{
    [SerializeField] LocationManager locationManager = null;
    [SerializeField] JsonDataLoader dataLoader = null;

    [Header("Route")]
    [SerializeField] bool useRouteOrder = true;
    [SerializeField] bool triggerOnlyCurrentRouteLocation = true;
    [SerializeField, Min(0.1f)] float checkIntervalSeconds = 1f;
    [SerializeField] bool autoAdvanceRouteOnTrigger = false;
    [SerializeField] bool allowRetriggerSameLocation = false;

    [Header("Editor Test")]
    [SerializeField] bool useMockLocationInEditor = true;
    [SerializeField] double mockLatitude = 38.703448d;
    [SerializeField] double mockLongitude = 39.257222d;

    readonly HashSet<string> triggeredLocationIds = new HashSet<string>();

    float nextCheckTime;
    bool isInitialized;
    bool routeCompleted;

    public event Action<LocationData, QuestData> OnLocationTriggered;
    public event Action<LocationData> OnTargetLocationChanged;
    public event Action OnRouteCompleted;

    public LocationData CurrentTargetLocation { get; private set; }
    public LocationData ActiveLocation { get; private set; }
    public QuestData ActiveQuest { get; private set; }
    public float DistanceToCurrentTarget { get; private set; } = -1f;
    public int CurrentRouteIndex { get; private set; }
    public bool IsInsideTriggerRadius { get; private set; }
    public bool HasActiveLocation
    {
        get { return ActiveLocation != null; }
    }

    public string StatusMessage { get; private set; } = "Location trigger bekleniyor";

    public bool UseRouteOrder
    {
        get { return useRouteOrder; }
    }

    public bool TriggerOnlyCurrentRouteLocation
    {
        get { return triggerOnlyCurrentRouteLocation; }
    }

    public bool UseMockLocationInEditor
    {
        get { return useMockLocationInEditor; }
    }

    void OnEnable()
    {
        ResolveReferences();
        SubscribeToDataLoaded();
    }

    void Start()
    {
        ResolveReferences();
        SubscribeToDataLoaded();
        InitializeAfterDataLoaded();
    }

    void Update()
    {
        if (Time.unscaledTime < nextCheckTime)
            return;

        nextCheckTime = Time.unscaledTime + checkIntervalSeconds;
        CheckCurrentLocation();
    }

    void OnDisable()
    {
        if (dataLoader != null)
            dataLoader.OnDataLoaded -= HandleDataLoaded;
    }

    void ResolveReferences()
    {
        if (locationManager == null)
            locationManager = LocationManager.Instance != null ? LocationManager.Instance : FindAnyObjectByType<LocationManager>();

        if (dataLoader == null)
            dataLoader = JsonDataLoader.Instance != null ? JsonDataLoader.Instance : FindAnyObjectByType<JsonDataLoader>();
    }

    void SubscribeToDataLoaded()
    {
        if (dataLoader == null)
            return;

        dataLoader.OnDataLoaded -= HandleDataLoaded;
        dataLoader.OnDataLoaded += HandleDataLoaded;
    }

    void HandleDataLoaded(JsonDataLoader loadedData)
    {
        isInitialized = false;
        InitializeAfterDataLoaded();
    }

    public void InitializeAfterDataLoaded()
    {
        ResolveReferences();
        SubscribeToDataLoaded();

        if (dataLoader == null)
        {
            StatusMessage = "JsonDataLoader bulunamadı";
            return;
        }

        if (!dataLoader.IsLoaded)
        {
            StatusMessage = "JSON data bekleniyor";
            return;
        }

        CurrentRouteIndex = Mathf.Max(0, CurrentRouteIndex);
        routeCompleted = false;

        var target = useRouteOrder ? GetLocationByRouteIndex(CurrentRouteIndex) : dataLoader.GetStartLocation();
        if (target == null)
        {
            StatusMessage = "Başlangıç hedefi bulunamadı";
            CurrentTargetLocation = null;
            isInitialized = false;
            return;
        }

        SetCurrentTarget(target, true);
        StatusMessage = "Hedef lokasyon hazır";
        isInitialized = true;
    }

    public void CheckCurrentLocation()
    {
        ResolveReferences();

        if (!isInitialized || CurrentTargetLocation == null)
            InitializeAfterDataLoaded();

        if (CurrentTargetLocation == null)
            return;

        if (!TryGetCurrentCoordinates(out var userLatitude, out var userLongitude))
            return;

        DistanceToCurrentTarget = (float)LocationManager.CalculateDistanceMeters(
            userLatitude,
            userLongitude,
            CurrentTargetLocation.latitude,
            CurrentTargetLocation.longitude);

        IsInsideTriggerRadius = DistanceToCurrentTarget <= CurrentTargetLocation.triggerRadiusMeters;
        StatusMessage = IsInsideTriggerRadius ? "Trigger alanı içinde" : "Hedefe ilerleniyor";

        if (!triggerOnlyCurrentRouteLocation &&
            TryFindAnyLocationInsideRadius(userLatitude, userLongitude, out var nearbyLocation, out var nearbyDistance))
        {
            StatusMessage = "Yakındaki lokasyon trigger alanı içinde";
            IsInsideTriggerRadius = true;
            if (nearbyLocation.id == CurrentTargetLocation.id)
                DistanceToCurrentTarget = nearbyDistance;

            TriggerLocation(nearbyLocation);
            return;
        }

        if (!IsInsideTriggerRadius)
            return;

        TriggerLocation(CurrentTargetLocation);
    }

    bool TryFindAnyLocationInsideRadius(double userLatitude, double userLongitude, out LocationData location, out float distanceMeters)
    {
        location = null;
        distanceMeters = float.MaxValue;

        if (dataLoader == null)
            return false;

        for (var i = 0; i < dataLoader.Locations.Count; i++)
        {
            var candidate = dataLoader.Locations[i];
            if (candidate == null || string.IsNullOrEmpty(candidate.id))
                continue;

            if (!allowRetriggerSameLocation && triggeredLocationIds.Contains(candidate.id))
                continue;

            var candidateDistance = (float)LocationManager.CalculateDistanceMeters(
                userLatitude,
                userLongitude,
                candidate.latitude,
                candidate.longitude);

            if (candidateDistance > candidate.triggerRadiusMeters || candidateDistance >= distanceMeters)
                continue;

            location = candidate;
            distanceMeters = candidateDistance;
        }

        return location != null;
    }

    bool TryGetCurrentCoordinates(out double latitude, out double longitude)
    {
#if UNITY_EDITOR
        if (useMockLocationInEditor)
        {
            latitude = mockLatitude;
            longitude = mockLongitude;
            return true;
        }
#endif

        latitude = 0d;
        longitude = 0d;

        if (locationManager == null)
        {
            StatusMessage = "LocationManager bulunamadı";
            return false;
        }

        if (!locationManager.IsLocationReady)
        {
            StatusMessage = "GPS konumu bekleniyor";
            return false;
        }

        latitude = locationManager.Latitude;
        longitude = locationManager.Longitude;
        return true;
    }

    public void TriggerLocation(LocationData location)
    {
        if (location == null || string.IsNullOrEmpty(location.id))
        {
            StatusMessage = "Tetiklenecek lokasyon bulunamadı";
            return;
        }

        if (triggerOnlyCurrentRouteLocation && CurrentTargetLocation != null && location.id != CurrentTargetLocation.id)
        {
            StatusMessage = "Lokasyon mevcut rota hedefi değil";
            return;
        }

        if (!allowRetriggerSameLocation && triggeredLocationIds.Contains(location.id))
        {
            StatusMessage = location.name + " daha önce tetiklendi";
            return;
        }

        ActiveLocation = location;
        ActiveQuest = dataLoader != null ? dataLoader.GetQuestByLocationId(location.id) : null;
        triggeredLocationIds.Add(location.id);

        StatusMessage = "Lokasyon tetiklendi: " + location.name;
        OnLocationTriggered?.Invoke(ActiveLocation, ActiveQuest);

        if (autoAdvanceRouteOnTrigger)
            AdvanceToNextRouteLocation();
    }

    public void AdvanceToNextRouteLocation()
    {
        if (dataLoader == null || !dataLoader.IsLoaded)
        {
            StatusMessage = "Rota ilerletmek için data bekleniyor";
            return;
        }

        var route = dataLoader.GetPrimaryRoute();
        if (!useRouteOrder || route == null || route.orderedLocationIds == null || route.orderedLocationIds.Length == 0)
        {
            var nextLocation = CurrentTargetLocation != null ? dataLoader.GetNextLocation(CurrentTargetLocation.id) : null;
            if (nextLocation == null)
            {
                CompleteRoute();
                return;
            }

            SetCurrentTarget(nextLocation, true);
            return;
        }

        if (CurrentRouteIndex + 1 >= route.orderedLocationIds.Length)
        {
            CompleteRoute();
            return;
        }

        CurrentRouteIndex++;
        var nextRouteLocation = GetLocationByRouteIndex(CurrentRouteIndex);
        if (nextRouteLocation == null)
        {
            CompleteRoute();
            return;
        }

        SetCurrentTarget(nextRouteLocation, true);
    }

    public bool SetCurrentTargetByLocationId(string locationId)
    {
        if (dataLoader == null || !dataLoader.IsLoaded)
        {
            StatusMessage = "Hedef seçmek için data bekleniyor";
            return false;
        }

        var location = dataLoader.GetLocationById(locationId);
        if (location == null)
        {
            StatusMessage = "Hedef lokasyon bulunamadı: " + locationId;
            return false;
        }

        CurrentRouteIndex = FindRouteIndex(locationId);
        SetCurrentTarget(location, true);
        routeCompleted = false;
        isInitialized = true;
        return true;
    }

    public void ResetRouteProgress()
    {
        triggeredLocationIds.Clear();
        ActiveLocation = null;
        ActiveQuest = null;
        DistanceToCurrentTarget = -1f;
        IsInsideTriggerRadius = false;
        CurrentRouteIndex = 0;
        routeCompleted = false;
        isInitialized = false;
        StatusMessage = "Rota sıfırlandı";
        InitializeAfterDataLoaded();
    }

    public string GetRouteProgressText()
    {
        var route = dataLoader != null ? dataLoader.GetPrimaryRoute() : null;
        var total = route != null && route.orderedLocationIds != null ? route.orderedLocationIds.Length : 0;

        if (total <= 0)
            return HasActiveLocation ? "1 / 1" : "0 / 0";

        var shownIndex = Mathf.Clamp(CurrentRouteIndex + 1, 1, total);
        if (routeCompleted)
            shownIndex = total;

        return shownIndex + " / " + total;
    }

    public QuestData GetCurrentTargetQuest()
    {
        if (CurrentTargetLocation == null || dataLoader == null)
            return null;

        return dataLoader.GetQuestByLocationId(CurrentTargetLocation.id);
    }

    public bool IsLocationTriggered(string locationId)
    {
        return !string.IsNullOrEmpty(locationId) && triggeredLocationIds.Contains(locationId);
    }

    LocationData GetLocationByRouteIndex(int routeIndex)
    {
        if (dataLoader == null)
            return null;

        var route = dataLoader.GetPrimaryRoute();
        if (route == null || route.orderedLocationIds == null || route.orderedLocationIds.Length == 0)
            return dataLoader.GetStartLocation();

        if (routeIndex < 0 || routeIndex >= route.orderedLocationIds.Length)
            return null;

        return dataLoader.GetLocationById(route.orderedLocationIds[routeIndex]);
    }

    int FindRouteIndex(string locationId)
    {
        var route = dataLoader != null ? dataLoader.GetPrimaryRoute() : null;
        if (route == null || route.orderedLocationIds == null)
            return 0;

        for (var i = 0; i < route.orderedLocationIds.Length; i++)
        {
            if (route.orderedLocationIds[i] == locationId)
                return i;
        }

        return 0;
    }

    void SetCurrentTarget(LocationData location, bool notify)
    {
        CurrentTargetLocation = location;
        IsInsideTriggerRadius = false;
        DistanceToCurrentTarget = -1f;
        StatusMessage = location != null ? "Hedef lokasyon: " + location.name : "Hedef lokasyon yok";

        if (notify && location != null)
            OnTargetLocationChanged?.Invoke(location);
    }

    void CompleteRoute()
    {
        if (routeCompleted)
            return;

        routeCompleted = true;
        StatusMessage = "Rota tamamlandı";
        OnRouteCompleted?.Invoke();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class JsonDataLoader : MonoBehaviour
{
    const string LocationsFileName = "locations.json";
    const string QuestsFileName = "quests.json";
    const string RouteOrderFileName = "route_order.json";
    const string CurrentTestStartLocationId = "current_test_start";
    const string CurrentTestFinalLocationId = "current_test_final";

    [SerializeField] bool loadOnStart = true;
    [SerializeField] DataEnvironmentConfig environmentConfig = null;
    [SerializeField] LocationManager locationManager = null;
    [SerializeField] string dataFolder = "Data";

    [Header("Current Location Test")]
    [SerializeField, Min(1f)] float currentLocationTestGpsWaitSeconds = 25f;
    [SerializeField] double currentLocationTestFinalLatitudeOffset = 0.0001d;
    [SerializeField] double currentLocationTestFinalLongitudeOffset = 0.0001d;
    [SerializeField, Min(1f)] float currentLocationTestTriggerRadiusMeters = 100f;
    [SerializeField] bool useEditorFallbackForCurrentLocationTest = true;
    [SerializeField] double editorFallbackLatitude = 38.680944d;
    [SerializeField] double editorFallbackLongitude = 39.195806d;

    readonly List<LocationData> locations = new List<LocationData>();
    readonly List<QuestData> quests = new List<QuestData>();
    readonly List<RouteOrderData> routeOrder = new List<RouteOrderData>();

    Coroutine loadRoutine;

    public static JsonDataLoader Instance { get; private set; }

    public event Action<JsonDataLoader> OnDataLoaded;

    public List<LocationData> Locations
    {
        get { return locations; }
    }

    public List<QuestData> Quests
    {
        get { return quests; }
    }

    public List<RouteOrderData> RouteOrder
    {
        get { return routeOrder; }
    }

    public bool IsLoaded { get; private set; }
    public string StatusMessage { get; private set; } = "Data yüklenmedi";
    public string ErrorMessage { get; private set; } = string.Empty;
    public string ActiveEnvironmentName
    {
        get { return GetActiveEnvironmentName(); }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        ResolveEnvironmentConfig();

        if (loadOnStart)
            LoadData();
    }

    public void LoadData()
    {
        if (loadRoutine != null)
            return;

        loadRoutine = StartCoroutine(LoadDataRoutine());
    }

    public void ReloadData()
    {
        if (loadRoutine != null)
            StopCoroutine(loadRoutine);

        loadRoutine = StartCoroutine(LoadDataRoutine());
    }

    IEnumerator LoadDataRoutine()
    {
        ResolveEnvironmentConfig();

        IsLoaded = false;
        ErrorMessage = string.Empty;
        StatusMessage = "JSON data yükleniyor. Environment: " + GetActiveEnvironmentName();
        locations.Clear();
        quests.Clear();
        routeOrder.Clear();

        string locationsJson = null;
        string questsJson = null;
        string routeOrderJson = null;
        string error = null;

        yield return ReadStreamingAssetsText(LocationsFileName, value => locationsJson = value, value => error = value);
        if (!string.IsNullOrEmpty(error))
        {
            FinishWithError(error);
            yield break;
        }

        yield return ReadStreamingAssetsText(QuestsFileName, value => questsJson = value, value => error = value);
        if (!string.IsNullOrEmpty(error))
        {
            FinishWithError(error);
            yield break;
        }

        yield return ReadStreamingAssetsText(RouteOrderFileName, value => routeOrderJson = value, value => error = value);
        if (!string.IsNullOrEmpty(error))
        {
            FinishWithError(error);
            yield break;
        }

        if (!TryParseLocations(locationsJson, out error) ||
            !TryParseQuests(questsJson, out error) ||
            !TryParseRouteOrder(routeOrderJson, out error))
        {
            FinishWithError(error);
            yield break;
        }

        if (IsCurrentLocationTestActive())
        {
            yield return ApplyCurrentLocationTestCoordinates(value => error = value);
            if (!string.IsNullOrEmpty(error))
            {
                FinishWithError(error);
                yield break;
            }
        }

        IsLoaded = true;
        if (!IsCurrentLocationTestActive())
            StatusMessage = "JSON data yüklendi. Environment: " + GetActiveEnvironmentName();

        loadRoutine = null;
        OnDataLoaded?.Invoke(this);
    }

    IEnumerator ApplyCurrentLocationTestCoordinates(Action<string> onError)
    {
        StatusMessage = "CurrentLocationTest active. GPS konumu bekleniyor.";

        if (!TryGetCurrentLocationTestCoordinates(out var currentLatitude, out var currentLongitude))
        {
            ResolveLocationManager();

            if (locationManager != null)
                locationManager.StartLocationService();

            var waitTime = 0f;
            while (!TryGetCurrentLocationTestCoordinates(out currentLatitude, out currentLongitude) &&
                waitTime < currentLocationTestGpsWaitSeconds)
            {
                waitTime += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (!TryGetCurrentLocationTestCoordinates(out currentLatitude, out currentLongitude))
        {
            onError?.Invoke("CurrentLocationTest için GPS konumu alınamadı.");
            yield break;
        }

        if (!OverrideCurrentLocationTestLocations(currentLatitude, currentLongitude, out var error))
        {
            onError?.Invoke(error);
            yield break;
        }

        StatusMessage = "CurrentLocationTest active. Test coordinates generated from current GPS location.";
    }

    bool TryGetCurrentLocationTestCoordinates(out double latitude, out double longitude)
    {
        ResolveLocationManager();

        if (locationManager != null && locationManager.IsLocationReady)
        {
            latitude = locationManager.Latitude;
            longitude = locationManager.Longitude;
            return true;
        }

#if UNITY_EDITOR
        if (useEditorFallbackForCurrentLocationTest)
        {
            latitude = editorFallbackLatitude;
            longitude = editorFallbackLongitude;
            return true;
        }
#endif

        latitude = 0d;
        longitude = 0d;
        return false;
    }

    bool OverrideCurrentLocationTestLocations(double currentLatitude, double currentLongitude, out string error)
    {
        error = string.Empty;

        var startLocation = GetLocationById(CurrentTestStartLocationId);
        if (startLocation == null)
        {
            error = "CurrentLocationTest başlangıç lokasyonu bulunamadı: " + CurrentTestStartLocationId;
            return false;
        }

        var finalLocation = GetLocationById(CurrentTestFinalLocationId);
        if (finalLocation == null)
        {
            error = "CurrentLocationTest final lokasyonu bulunamadı: " + CurrentTestFinalLocationId;
            return false;
        }

        startLocation.latitude = currentLatitude;
        startLocation.longitude = currentLongitude;
        startLocation.triggerRadiusMeters = currentLocationTestTriggerRadiusMeters;

        finalLocation.latitude = currentLatitude + currentLocationTestFinalLatitudeOffset;
        finalLocation.longitude = currentLongitude + currentLocationTestFinalLongitudeOffset;
        finalLocation.triggerRadiusMeters = currentLocationTestTriggerRadiusMeters;

        return true;
    }

    IEnumerator ReadStreamingAssetsText(string fileName, Action<string> onSuccess, Action<string> onError)
    {
        var uri = BuildStreamingAssetsUri(fileName);
        using (var request = UnityWebRequest.Get(uri))
        {
            yield return request.SendWebRequest();

            if (HasRequestError(request))
            {
                onError?.Invoke(BuildEnvironmentRelativePath(fileName) + " okunamadı: " + request.error);
                yield break;
            }

            onSuccess?.Invoke(request.downloadHandler.text);
        }
    }

    string BuildStreamingAssetsUri(string fileName)
    {
        var path = Path.Combine(GetDataRootPath(), fileName).Replace("\\", "/");
        if (path.Contains("://"))
            return path;

        return "file://" + path;
    }

    string GetDataRootPath()
    {
        ResolveEnvironmentConfig();

        if (environmentConfig != null)
            return environmentConfig.GetDataRootPath();

        return Path.Combine(Application.streamingAssetsPath, GetDataFolderName(), DataEnvironment.CurrentLocationTest.ToString()).Replace("\\", "/");
    }

    string BuildEnvironmentRelativePath(string fileName)
    {
        return Path.Combine(GetDataFolderName(), GetActiveEnvironmentName(), fileName).Replace("\\", "/");
    }

    string GetActiveEnvironmentName()
    {
        ResolveEnvironmentConfig();
        return environmentConfig != null ? environmentConfig.GetEnvironmentFolderName() : DataEnvironment.CurrentLocationTest.ToString();
    }

    string GetDataFolderName()
    {
        return string.IsNullOrEmpty(dataFolder) ? "Data" : dataFolder;
    }

    void ResolveEnvironmentConfig()
    {
        if (environmentConfig != null)
            return;

        var configs = FindObjectsByType<DataEnvironmentConfig>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        environmentConfig = configs.Length > 0 ? configs[0] : null;
    }

    void ResolveLocationManager()
    {
        if (locationManager == null)
            locationManager = LocationManager.Instance != null ? LocationManager.Instance : FindAnyObjectByType<LocationManager>();
    }

    bool IsCurrentLocationTestActive()
    {
        return GetActiveEnvironmentName() == DataEnvironment.CurrentLocationTest.ToString();
    }

    static bool HasRequestError(UnityWebRequest request)
    {
#if UNITY_2020_2_OR_NEWER
        return request.result != UnityWebRequest.Result.Success;
#else
        return request.isNetworkError || request.isHttpError;
#endif
    }

    bool TryParseLocations(string json, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(json))
        {
            error = LocationsFileName + " boş veya okunamadı.";
            return false;
        }

        var wrapper = JsonUtility.FromJson<LocationDataCollection>(json);
        if (wrapper == null || wrapper.locations == null)
        {
            error = LocationsFileName + " beklenen wrapper alanını içermiyor: locations.";
            return false;
        }

        locations.AddRange(wrapper.locations);
        return true;
    }

    bool TryParseQuests(string json, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(json))
        {
            error = QuestsFileName + " boş veya okunamadı.";
            return false;
        }

        var wrapper = JsonUtility.FromJson<QuestDataCollection>(json);
        if (wrapper == null || wrapper.quests == null)
        {
            error = QuestsFileName + " beklenen wrapper alanını içermiyor: quests.";
            return false;
        }

        quests.AddRange(wrapper.quests);
        return true;
    }

    bool TryParseRouteOrder(string json, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(json))
        {
            error = RouteOrderFileName + " boş veya okunamadı.";
            return false;
        }

        var wrapper = JsonUtility.FromJson<RouteOrderDataCollection>(json);
        if (wrapper == null || wrapper.routes == null)
        {
            error = RouteOrderFileName + " beklenen wrapper alanını içermiyor: routes.";
            return false;
        }

        routeOrder.AddRange(wrapper.routes);
        return true;
    }

    void FinishWithError(string error)
    {
        IsLoaded = false;
        ErrorMessage = string.IsNullOrEmpty(error) ? "Bilinmeyen data yükleme hatası." : error;
        StatusMessage = "JSON data yükleme hatası. Environment: " + GetActiveEnvironmentName();
        loadRoutine = null;
    }

    public LocationData GetLocationById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        for (var i = 0; i < locations.Count; i++)
        {
            var location = locations[i];
            if (location != null && location.id == id)
                return location;
        }

        return null;
    }

    public QuestData GetQuestById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        for (var i = 0; i < quests.Count; i++)
        {
            var quest = quests[i];
            if (quest != null && quest.id == id)
                return quest;
        }

        return null;
    }

    public QuestData GetQuestByLocationId(string locationId)
    {
        if (string.IsNullOrEmpty(locationId))
            return null;

        for (var i = 0; i < quests.Count; i++)
        {
            var quest = quests[i];
            if (quest != null && quest.locationId == locationId)
                return quest;
        }

        return null;
    }

    public LocationData GetNextLocation(string currentLocationId)
    {
        if (string.IsNullOrEmpty(currentLocationId))
            return null;

        var route = GetPrimaryRoute();
        if (route != null && route.orderedLocationIds != null)
        {
            for (var i = 0; i < route.orderedLocationIds.Length - 1; i++)
            {
                if (route.orderedLocationIds[i] == currentLocationId)
                    return GetLocationById(route.orderedLocationIds[i + 1]);
            }
        }

        var quest = GetQuestByLocationId(currentLocationId);
        if (quest != null && !string.IsNullOrEmpty(quest.nextLocationId))
            return GetLocationById(quest.nextLocationId);

        return null;
    }

    public LocationData GetStartLocation()
    {
        for (var i = 0; i < locations.Count; i++)
        {
            var location = locations[i];
            if (location != null && location.isStartPoint)
                return location;
        }

        var route = GetPrimaryRoute();
        if (route != null && route.orderedLocationIds != null && route.orderedLocationIds.Length > 0)
            return GetLocationById(route.orderedLocationIds[0]);

        return locations.Count > 0 ? locations[0] : null;
    }

    public RouteOrderData GetPrimaryRoute()
    {
        return routeOrder.Count > 0 ? routeOrder[0] : null;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    [Serializable]
    class LocationDataCollection
    {
        public LocationData[] locations = null;
    }

    [Serializable]
    class QuestDataCollection
    {
        public QuestData[] quests = null;
    }

    [Serializable]
    class RouteOrderDataCollection
    {
        public RouteOrderData[] routes = null;
    }
}

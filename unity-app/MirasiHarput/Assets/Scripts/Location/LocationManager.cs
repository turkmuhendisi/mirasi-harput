using System;
using System.Collections;
using UnityEngine;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class LocationManager : MonoBehaviour
{
    const double EarthRadiusMeters = 6371000d;
    const string FineLocationPermission = "android.permission.ACCESS_FINE_LOCATION";
    const string CoarseLocationPermission = "android.permission.ACCESS_COARSE_LOCATION";

    [SerializeField, Min(1f)] float desiredAccuracyInMeters = 10f;
    [SerializeField, Min(0f)] float updateDistanceInMeters = 1f;
    [SerializeField, Min(1f)] float maxWaitSeconds = 20f;

    Coroutine startLocationRoutine;
    double lastTimestamp = double.NaN;

    public static LocationManager Instance { get; private set; }

    public event Action<LocationManager> OnLocationUpdated;

    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public float HorizontalAccuracy { get; private set; }
    public double Timestamp { get; private set; }
    public bool IsLocationReady { get; private set; }
    public string StatusMessage { get; private set; } = "GPS başlatılıyor";

    public float DesiredAccuracyInMeters => desiredAccuracyInMeters;
    public float UpdateDistanceInMeters => updateDistanceInMeters;
    public float MaxWaitSeconds => maxWaitSeconds;

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
        StartLocationService();
    }

    void Update()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            StatusMessage = "GPS aktif";
            UpdateLocationFromService(false);
            return;
        }

        if (IsLocationReady && Input.location.status == LocationServiceStatus.Stopped)
        {
            IsLocationReady = false;
            StatusMessage = "GPS kapalı";
        }
    }

    public void StartLocationService()
    {
        if (startLocationRoutine != null)
            return;

        startLocationRoutine = StartCoroutine(StartLocationServiceRoutine());
    }

    IEnumerator StartLocationServiceRoutine()
    {
        StatusMessage = "GPS başlatılıyor";
        IsLocationReady = false;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!HasAndroidLocationPermission())
        {
            StatusMessage = "Konum izni bekleniyor";
            Permission.RequestUserPermissions(new[] { FineLocationPermission, CoarseLocationPermission });

            var permissionWaitTime = 0f;
            while (!HasAndroidLocationPermission() && permissionWaitTime < maxWaitSeconds)
            {
                permissionWaitTime += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!HasAndroidLocationPermission())
            {
                StatusMessage = "Konum izni verilmedi";
                startLocationRoutine = null;
                yield break;
            }
        }
#endif

        if (!Input.location.isEnabledByUser)
        {
            StatusMessage = "GPS kapalı";
            startLocationRoutine = null;
            yield break;
        }

        Input.location.Start(desiredAccuracyInMeters, updateDistanceInMeters);

        var waitTime = 0f;
        while (Input.location.status == LocationServiceStatus.Initializing && waitTime < maxWaitSeconds)
        {
            StatusMessage = "GPS başlatılıyor";
            waitTime += Time.unscaledDeltaTime;
            yield return null;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            StatusMessage = "GPS başlatılamadı";
            startLocationRoutine = null;
            yield break;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            StatusMessage = "GPS zaman aşımı";
            Input.location.Stop();
            startLocationRoutine = null;
            yield break;
        }

        StatusMessage = "GPS aktif";
        UpdateLocationFromService(true);
        startLocationRoutine = null;
    }

    void UpdateLocationFromService(bool forceNotify)
    {
        var locationData = Input.location.lastData;
        if (!forceNotify && AreTimestampsEqual(locationData.timestamp, lastTimestamp))
            return;

        Latitude = locationData.latitude;
        Longitude = locationData.longitude;
        HorizontalAccuracy = locationData.horizontalAccuracy;
        Timestamp = locationData.timestamp;
        IsLocationReady = true;
        lastTimestamp = locationData.timestamp;

        OnLocationUpdated?.Invoke(this);
    }

    static bool AreTimestampsEqual(double first, double second)
    {
        if (double.IsNaN(first) || double.IsNaN(second))
            return false;

        return Math.Abs(first - second) < 0.0001d;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    static bool HasAndroidLocationPermission()
    {
        return Permission.HasUserAuthorizedPermission(FineLocationPermission) ||
            Permission.HasUserAuthorizedPermission(CoarseLocationPermission);
    }
#endif

    public static double CalculateDistanceMeters(double firstLatitude, double firstLongitude, double secondLatitude, double secondLongitude)
    {
        var firstLatitudeRadians = DegreesToRadians(firstLatitude);
        var secondLatitudeRadians = DegreesToRadians(secondLatitude);
        var latitudeDeltaRadians = DegreesToRadians(secondLatitude - firstLatitude);
        var longitudeDeltaRadians = DegreesToRadians(secondLongitude - firstLongitude);

        var latitudeSin = Math.Sin(latitudeDeltaRadians / 2d);
        var longitudeSin = Math.Sin(longitudeDeltaRadians / 2d);
        var haversine = latitudeSin * latitudeSin +
            Math.Cos(firstLatitudeRadians) * Math.Cos(secondLatitudeRadians) * longitudeSin * longitudeSin;

        haversine = Math.Min(1d, Math.Max(0d, haversine));
        var centralAngle = 2d * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1d - haversine));
        return EarthRadiusMeters * centralAngle;
    }

    static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180d;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (Input.location.status == LocationServiceStatus.Running ||
            Input.location.status == LocationServiceStatus.Initializing)
        {
            Input.location.Stop();
        }
    }
}

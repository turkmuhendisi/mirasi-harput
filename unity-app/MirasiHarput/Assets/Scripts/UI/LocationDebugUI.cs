using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;

public class LocationDebugUI : MonoBehaviour
{
    [SerializeField] LocationManager locationManager = null;
    [SerializeField] LocationTriggerManager locationTriggerManager = null;
    [SerializeField] JsonDataLoader dataLoader = null;

    [Header("Text References")]
    [SerializeField] TMP_Text statusText = null;
    [SerializeField] TMP_Text latitudeText = null;
    [SerializeField] TMP_Text longitudeText = null;
    [SerializeField] TMP_Text accuracyText = null;
    [SerializeField] TMP_Text targetText = null;
    [SerializeField] TMP_Text distanceText = null;

    [SerializeField, Min(0.05f)] float refreshInterval = 0.25f;

    bool subscribedToLocationUpdates;
    bool subscribedToDataLoaded;
    float nextRefreshTime;

    void OnEnable()
    {
        ResolveReferences();
        SubscribeToLocationUpdates();
        Refresh();
    }

    void Start()
    {
        ResolveReferences();
        SubscribeToLocationUpdates();
        Refresh();
        StartCoroutine(RefreshAfterCanvasReady());
    }

    IEnumerator RefreshAfterCanvasReady()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
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
        if (subscribedToLocationUpdates && locationManager != null)
            locationManager.OnLocationUpdated -= HandleLocationUpdated;

        if (subscribedToDataLoaded && dataLoader != null)
            dataLoader.OnDataLoaded -= HandleDataLoaded;

        subscribedToLocationUpdates = false;
        subscribedToDataLoaded = false;
    }

    void ResolveReferences()
    {
        if (locationManager == null)
        {
            locationManager = LocationManager.Instance;
            if (locationManager == null)
                locationManager = FindAnyObjectByType<LocationManager>();
        }

        if (locationTriggerManager == null)
        {
            var triggers = FindObjectsByType<LocationTriggerManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            locationTriggerManager = triggers.Length > 0 ? triggers[0] : null;
        }

        if (dataLoader == null)
        {
            if (JsonDataLoader.Instance != null)
                dataLoader = JsonDataLoader.Instance;
            else
            {
                var loaders = FindObjectsByType<JsonDataLoader>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                dataLoader = loaders.Length > 0 ? loaders[0] : null;
            }
        }
    }

    void SubscribeToLocationUpdates()
    {
        if (!subscribedToLocationUpdates && locationManager != null)
        {
            locationManager.OnLocationUpdated += HandleLocationUpdated;
            subscribedToLocationUpdates = true;
        }

        if (!subscribedToDataLoaded && dataLoader != null)
        {
            dataLoader.OnDataLoaded += HandleDataLoaded;
            subscribedToDataLoaded = true;
        }
    }

    void HandleLocationUpdated(LocationManager updatedLocationManager)
    {
        Refresh();
    }

    void HandleDataLoaded(JsonDataLoader loader)
    {
        dataLoader = loader;
        Refresh();
    }

    void Refresh()
    {
        ResolveReferences();
        SubscribeToLocationUpdates();

        if (locationManager == null)
        {
            SetText(statusText, "Durum: LocationManager bulunamadı");
            SetText(latitudeText, "Latitude: -");
            SetText(longitudeText, "Longitude: -");
            SetText(accuracyText, "Accuracy: -");
            SetText(targetText, "Hedef: -");
            SetText(distanceText, "Mesafe: -");
            return;
        }

        var environmentLabel = dataLoader != null ? dataLoader.ActiveEnvironmentName : "-";
        var dataStatus = dataLoader != null
            ? (dataLoader.IsLoaded ? "veri yüklendi" : dataLoader.StatusMessage)
            : "JsonDataLoader yok";
        SetText(statusText, $"Durum: {locationManager.StatusMessage} | Ortam: {environmentLabel} | {dataStatus}");

        if (!locationManager.IsLocationReady)
        {
            SetText(latitudeText, "Latitude: -");
            SetText(longitudeText, "Longitude: -");
            SetText(accuracyText, "Accuracy: -");
            SetText(targetText, "Hedef: GPS bekleniyor");
            SetText(distanceText, "Mesafe: -");
            return;
        }

        SetText(latitudeText, $"Latitude: {FormatCoordinate(locationManager.Latitude)}");
        SetText(longitudeText, $"Longitude: {FormatCoordinate(locationManager.Longitude)}");
        SetText(accuracyText, $"Accuracy: {locationManager.HorizontalAccuracy.ToString("F1", CultureInfo.InvariantCulture)} m");

        var target = locationTriggerManager != null ? locationTriggerManager.CurrentTargetLocation : null;
        if (target == null && dataLoader != null && dataLoader.IsLoaded)
            target = dataLoader.GetStartLocation();

        if (target == null)
        {
            SetText(targetText, "Hedef: -");
            SetText(distanceText, "Mesafe: -");
            return;
        }

        SetText(targetText, $"Hedef: {target.name}");

        var distanceMeters = locationTriggerManager != null && locationTriggerManager.DistanceToCurrentTarget >= 0f
            ? locationTriggerManager.DistanceToCurrentTarget
            : (float)LocationManager.CalculateDistanceMeters(
                locationManager.Latitude,
                locationManager.Longitude,
                target.latitude,
                target.longitude);

        var inside = locationTriggerManager != null && locationTriggerManager.IsInsideTriggerRadius;
        SetText(distanceText, $"Mesafe: {distanceMeters.ToString("F1", CultureInfo.InvariantCulture)} m | Tetik: {(inside ? "Evet" : "Hayır")}");
    }

    static string FormatCoordinate(double coordinate)
    {
        return coordinate.ToString("F6", CultureInfo.InvariantCulture);
    }

    static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}

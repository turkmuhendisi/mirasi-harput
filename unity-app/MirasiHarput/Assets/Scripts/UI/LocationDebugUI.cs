using System.Globalization;
using TMPro;
using UnityEngine;

public class LocationDebugUI : MonoBehaviour
{
    [SerializeField] LocationManager locationManager;

    [Header("Text References")]
    [SerializeField] TMP_Text statusText = null;
    [SerializeField] TMP_Text latitudeText = null;
    [SerializeField] TMP_Text longitudeText = null;
    [SerializeField] TMP_Text accuracyText = null;
    [SerializeField] TMP_Text targetText = null;
    [SerializeField] TMP_Text distanceText = null;

    [Header("Target")]
    [SerializeField] string targetName = "Harput Kalesi";
    [SerializeField] double targetLatitude = 38.703448d;
    [SerializeField] double targetLongitude = 39.257222d;
    [SerializeField, Min(0.05f)] float refreshInterval = 0.25f;

    bool subscribedToLocationUpdates;
    float nextRefreshTime;

    void OnEnable()
    {
        ResolveLocationManager();
        SubscribeToLocationUpdates();
        Refresh();
    }

    void Start()
    {
        ResolveLocationManager();
        SubscribeToLocationUpdates();
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

        subscribedToLocationUpdates = false;
    }

    void ResolveLocationManager()
    {
        if (locationManager != null)
            return;

        locationManager = LocationManager.Instance;
        if (locationManager == null)
            locationManager = FindAnyObjectByType<LocationManager>();
    }

    void SubscribeToLocationUpdates()
    {
        if (subscribedToLocationUpdates || locationManager == null)
            return;

        locationManager.OnLocationUpdated += HandleLocationUpdated;
        subscribedToLocationUpdates = true;
    }

    void HandleLocationUpdated(LocationManager updatedLocationManager)
    {
        Refresh();
    }

    void Refresh()
    {
        ResolveLocationManager();
        SubscribeToLocationUpdates();

        SetText(targetText, $"Hedef: {targetName}");

        if (locationManager == null)
        {
            SetText(statusText, "Durum: LocationManager bulunamadı");
            SetText(latitudeText, "Latitude: -");
            SetText(longitudeText, "Longitude: -");
            SetText(accuracyText, "Accuracy: -");
            SetText(distanceText, "Mesafe: -");
            return;
        }

        SetText(statusText, $"Durum: {locationManager.StatusMessage}");

        if (!locationManager.IsLocationReady)
        {
            SetText(latitudeText, "Latitude: -");
            SetText(longitudeText, "Longitude: -");
            SetText(accuracyText, "Accuracy: -");
            SetText(distanceText, "Mesafe: GPS bekleniyor");
            return;
        }

        SetText(latitudeText, $"Latitude: {FormatCoordinate(locationManager.Latitude)}");
        SetText(longitudeText, $"Longitude: {FormatCoordinate(locationManager.Longitude)}");
        SetText(accuracyText, $"Accuracy: {locationManager.HorizontalAccuracy.ToString("F1", CultureInfo.InvariantCulture)} m");

        var distanceMeters = LocationManager.CalculateDistanceMeters(
            locationManager.Latitude,
            locationManager.Longitude,
            targetLatitude,
            targetLongitude);

        SetText(distanceText, $"Mesafe: {distanceMeters.ToString("F1", CultureInfo.InvariantCulture)} m");
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

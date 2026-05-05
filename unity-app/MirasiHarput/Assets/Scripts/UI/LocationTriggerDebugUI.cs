using System.Globalization;
using TMPro;
using UnityEngine;

public class LocationTriggerDebugUI : MonoBehaviour
{
    [SerializeField] LocationTriggerManager triggerManager = null;

    [Header("Text References")]
    [SerializeField] TMP_Text triggerStatusText = null;
    [SerializeField] TMP_Text currentTargetText = null;
    [SerializeField] TMP_Text activeLocationText = null;
    [SerializeField] TMP_Text activeQuestText = null;
    [SerializeField] TMP_Text distanceToTargetText = null;
    [SerializeField] TMP_Text triggerRadiusText = null;
    [SerializeField] TMP_Text routeProgressText = null;
    [SerializeField] TMP_Text insideTriggerText = null;

    [SerializeField, Min(0.05f)] float refreshInterval = 0.25f;

    float nextRefreshTime;

    void OnEnable()
    {
        ResolveTriggerManager();
        Refresh();
    }

    void Start()
    {
        ResolveTriggerManager();
        Refresh();
    }

    void Update()
    {
        if (Time.unscaledTime < nextRefreshTime)
            return;

        nextRefreshTime = Time.unscaledTime + refreshInterval;
        Refresh();
    }

    void ResolveTriggerManager()
    {
        if (triggerManager != null)
            return;

        triggerManager = FindAnyObjectByType<LocationTriggerManager>();
    }

    void Refresh()
    {
        ResolveTriggerManager();

        if (triggerManager == null)
        {
            SetText(triggerStatusText, "Trigger status: LocationTriggerManager bulunamadı");
            SetText(currentTargetText, "Current target: -");
            SetText(activeLocationText, "Active location: -");
            SetText(activeQuestText, "Active quest: -");
            SetText(distanceToTargetText, "Distance: -");
            SetText(triggerRadiusText, "Trigger radius: -");
            SetText(routeProgressText, "Route progress: -");
            SetText(insideTriggerText, "Inside trigger: -");
            return;
        }

        var currentTarget = triggerManager.CurrentTargetLocation;
        var activeLocation = triggerManager.ActiveLocation;
        var activeQuest = triggerManager.ActiveQuest;

        SetText(triggerStatusText, "Trigger status: " + triggerManager.StatusMessage);
        SetText(currentTargetText, "Current target: " + (currentTarget != null ? currentTarget.name : "-"));
        SetText(activeLocationText, "Active location: " + (activeLocation != null ? activeLocation.name : "-"));
        SetText(activeQuestText, "Active quest: " + (activeQuest != null ? activeQuest.title : "-"));
        SetText(distanceToTargetText, "Distance: " + FormatDistance(triggerManager.DistanceToCurrentTarget));
        SetText(triggerRadiusText, "Trigger radius: " + (currentTarget != null ? currentTarget.triggerRadiusMeters.ToString("F1", CultureInfo.InvariantCulture) + " m" : "-"));
        SetText(routeProgressText, "Route progress: " + triggerManager.GetRouteProgressText());
        SetText(insideTriggerText, "Inside trigger: " + (triggerManager.IsInsideTriggerRadius ? "Evet" : "Hayır"));
    }

    static string FormatDistance(float distanceMeters)
    {
        if (distanceMeters < 0f)
            return "-";

        return distanceMeters.ToString("F1", CultureInfo.InvariantCulture) + " m";
    }

    static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}

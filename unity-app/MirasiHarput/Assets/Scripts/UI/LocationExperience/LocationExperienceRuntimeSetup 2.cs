using UnityEngine;
using UnityEngine.EventSystems;

public static class LocationExperienceRuntimeSetup
{
    public static void EnsureFlowController()
    {
        if (!IsQrLocationExperienceActive())
            return;

        DisableLegacyFlowControllers();
        EnsureEventSystem();

        var existing = DataEnvironmentModeActivator.FindSceneObjectIncludingInactivePublic("LocationExperienceOverlay");
        if (existing != null)
            Object.DestroyImmediate(existing);

        var instance = LocationExperienceUIBuilder.Build();
        instance.name = "LocationExperienceOverlay";
        DataEnvironmentModeActivator.EnsureQrRouteUiVisible();
    }

    static bool IsQrLocationExperienceActive()
    {
        var configs = Object.FindObjectsByType<DataEnvironmentConfig>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (configs == null)
            return false;

        for (var i = 0; i < configs.Length; i++)
        {
            if (configs[i] != null && configs[i].UsesQrLocationExperienceMvp())
                return true;
        }

        return false;
    }

    static void DisableLegacyFlowControllers()
    {
        var legacyControllers = Object.FindObjectsByType<QrFlowController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < legacyControllers.Length; i++)
        {
            if (legacyControllers[i] != null)
                legacyControllers[i].enabled = false;
        }

        DataEnvironmentModeActivator.SetObjectActivePublic("QrFlowOverlay", false);
    }

    static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
            return;

        var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Object.DontDestroyOnLoad(eventSystemGo);
    }
}

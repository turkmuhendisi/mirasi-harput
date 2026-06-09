using UnityEngine;

public static class QrLocationTriggerRuntimeSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnAfterSceneLoad()
    {
        EnsureComponents();
    }

    public static void EnsureComponents()
    {
        var triggerManager = Object.FindAnyObjectByType<LocationTriggerManager>(FindObjectsInactive.Include);
        if (triggerManager == null)
            return;

        ApplyTriggerSourceFromEnvironment(triggerManager);

        var host = triggerManager.gameObject;
        EnsureComponent<QrLocationTriggerBridge>(host);
        var scanService = EnsureComponent<QrCodeScanService>(host);
        EnsureComponent<QrLocationNpcPresenter>(host);

        if (scanService != null)
            scanService.enabled = triggerManager.UsesQrTriggerMode;
    }

    static void ApplyTriggerSourceFromEnvironment(LocationTriggerManager triggerManager)
    {
        var configs = Object.FindObjectsByType<DataEnvironmentConfig>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (configs == null || configs.Length == 0)
            return;

        for (var i = 0; i < configs.Length; i++)
        {
            var config = configs[i];
            if (config == null)
                continue;

            triggerManager.SetTriggerSource(config.UsesQrTriggerMode()
                ? LocationTriggerSource.QrCode
                : LocationTriggerSource.GpsProximity);
            return;
        }
    }

    static T EnsureComponent<T>(GameObject host) where T : Component
    {
        var existing = host.GetComponent<T>();
        if (existing != null)
            return existing;

        return host.AddComponent<T>();
    }
}

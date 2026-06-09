using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Harput QR modunda QrFlowUI prefab'ını yükler veya sahnedeki QrFlowUIView örneğini kullanır.
/// </summary>
public static class QrFlowRuntimeSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnAfterSceneLoad()
    {
        EnsureFlowController();
    }

    public static void EnsureFlowController()
    {
        if (!IsQrModeActive())
            return;

        RemoveLegacyFlowControllerOnUiCanvas();
        EnsureEventSystem();

        if (UnityEngine.Object.FindAnyObjectByType<QrFlowController>(FindObjectsInactive.Include) != null)
            return;

        var sceneView = UnityEngine.Object.FindAnyObjectByType<QrFlowUIView>(FindObjectsInactive.Include);
        if (sceneView != null)
        {
            if (sceneView.GetComponent<QrFlowController>() == null)
                sceneView.gameObject.AddComponent<QrFlowController>();
            return;
        }

        var prefab = Resources.Load<GameObject>(QrFlowUIView.PrefabResourcesPath);
        if (prefab == null)
        {
            Debug.LogWarning(
                "[QrFlow] Prefab bulunamadı: Resources/" + QrFlowUIView.PrefabResourcesPath +
                ". Unity menüsünden Mirasi Harput → UI → Create QR Flow UI Prefabs çalıştırın.");
            return;
        }

        var instance = UnityEngine.Object.Instantiate(prefab);
        instance.name = "QrFlowOverlay";
    }

    static bool IsQrModeActive()
    {
        var configs = UnityEngine.Object.FindObjectsByType<DataEnvironmentConfig>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (configs == null)
            return false;

        for (var i = 0; i < configs.Length; i++)
        {
            if (configs[i] != null && configs[i].UsesQrLocationExperienceMvp())
                return false;
        }

        for (var i = 0; i < configs.Length; i++)
        {
            if (configs[i] != null && configs[i].UsesQrTriggerMode())
                return true;
        }

        return false;
    }

    static void RemoveLegacyFlowControllerOnUiCanvas()
    {
        var controllers = UnityEngine.Object.FindObjectsByType<QrFlowController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < controllers.Length; i++)
        {
            var controller = controllers[i];
            if (controller == null)
                continue;

            var hostCanvas = controller.GetComponent<Canvas>();
            if (hostCanvas == null)
                hostCanvas = controller.GetComponentInParent<Canvas>();

            if (hostCanvas != null && hostCanvas.gameObject.name == "UI")
                UnityEngine.Object.Destroy(controller);
        }
    }

    static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
            return;

        var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        UnityEngine.Object.DontDestroyOnLoad(eventSystemGo);
    }
}

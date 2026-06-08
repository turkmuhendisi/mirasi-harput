using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Sahne yüklendiğinde DataEnvironmentConfig'e göre iç mekân veya GPS rota modunu açar.
/// Kapalı (inactive) objeler GameObject.Find ile bulunamaz; bileşen tipiyle aranır.
/// </summary>
public static class DataEnvironmentModeActivator
{
    public static bool IsGpsRouteModeActive { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnSceneLoaded()
    {
        IsGpsRouteModeActive = false;

        var configs = UnityEngine.Object.FindObjectsByType<DataEnvironmentConfig>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (configs == null || configs.Length == 0)
            return;

        for (var i = 0; i < configs.Length; i++)
        {
            if (configs[i] != null)
                configs[i].ApplyPlayMode();
        }

        if (!IsQrLocationExperienceMvpActive())
            OutdoorGpsNpcViewRuntimeBootstrap.EnsureHost();
    }

    static bool IsQrLocationExperienceMvpActive()
    {
        var configs = UnityEngine.Object.FindObjectsByType<DataEnvironmentConfig>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (configs == null)
            return false;

        for (var i = 0; i < configs.Length; i++)
        {
            if (configs[i] != null && configs[i].UsesQrLocationExperienceMvp())
                return true;
        }

        return false;
    }

    internal static void SetGpsRouteModeActive(bool value)
    {
        IsGpsRouteModeActive = value;
    }

    /// <summary>
    /// GPS rota modunda sol üst konum/tetik debug panellerinin AR üzerinde görünür olmasını sağlar.
    /// </summary>
    /// <summary>
    /// QR rota modunda UI canvas'ını AR üzerinde görünür ve tıklanabilir yapar.
    /// MainScene'de UI kökünün scale'i (0,0,0) olduğu için bu adım zorunludur.
    /// </summary>
    internal static void EnsureQrRouteUiVisible()
    {
        SetObjectActive("UI", true);
        ConfigureRouteUiCanvas(FindSceneObjectIncludingInactive("UI"), 500);

        var flowRoot = FindSceneObjectIncludingInactive("LocationExperienceOverlay");
        if (flowRoot == null)
            flowRoot = FindSceneObjectIncludingInactive("QrFlowOverlay");

        if (flowRoot != null)
            ConfigureRouteUiCanvas(flowRoot, 2000);
    }

    public static void SetObjectActivePublic(string objectName, bool active)
    {
        SetObjectActive(objectName, active);
    }

    public static GameObject FindSceneObjectIncludingInactivePublic(string objectName)
    {
        return FindSceneObjectIncludingInactive(objectName);
    }

    internal static void EnsureGpsRouteDebugUiVisible()
    {
        SetObjectActive("UI", true);
        SetObjectActive("LocationDebugPanel", true);
        SetObjectActive("LocationTriggerDebugPanel", true);

        ConfigureRouteUiCanvas(FindSceneObjectIncludingInactive("UI"), 500);

        BringPanelToFront("LocationDebugPanel");
        BringPanelToFront("LocationTriggerDebugPanel");

        SetComponentsActive<LocationDebugUI>(true);
    }

    static void ConfigureRouteUiCanvas(GameObject uiRoot, int sortingOrder)
    {
        if (uiRoot == null)
            return;

        var rectTransform = uiRoot.GetComponent<RectTransform>();
        if (rectTransform != null && rectTransform.localScale.sqrMagnitude < 0.01f)
            rectTransform.localScale = Vector3.one;

        var canvas = uiRoot.GetComponent<Canvas>();
        if (canvas != null)
        {
            var keepOverlay = uiRoot.name == "LocationExperienceOverlay";

            if (!keepOverlay)
            {
                var arCamera = Camera.main;
                if (arCamera == null)
                    arCamera = Object.FindAnyObjectByType<Camera>();

                if (arCamera != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = arCamera;
                    canvas.planeDistance = 0.5f;
                }
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }

        if (uiRoot.GetComponent<GraphicRaycaster>() == null)
            uiRoot.AddComponent<GraphicRaycaster>();

        var scaler = uiRoot.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.enabled = false;
            scaler.enabled = true;
        }

        Canvas.ForceUpdateCanvases();
    }

    static void BringPanelToFront(string panelName)
    {
        var panel = FindSceneObjectIncludingInactive(panelName);
        if (panel == null)
            return;

        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
    }

    internal static void SetComponentsActive<T>(bool active) where T : Component
    {
        var components = UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < components.Length; i++)
        {
            if (components[i] != null)
                components[i].gameObject.SetActive(active);
        }
    }

    internal static void SetObjectActive(string objectName, bool active)
    {
        if (string.IsNullOrEmpty(objectName))
            return;

        var target = FindSceneObjectIncludingInactive(objectName);
        if (target != null)
            target.SetActive(active);
    }

    /// <summary>
    /// Tip adıyla MonoBehaviour bul ve aktif/pasif et. Hem GameObject hem behaviour pasif edilir.
    /// </summary>
    internal static void SetComponentBehavioursActiveByTypeName(string typeName, bool active)
    {
        if (string.IsNullOrEmpty(typeName))
            return;

        var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < behaviours.Length; i++)
        {
            var b = behaviours[i];
            if (b == null)
                continue;

            if (b.GetType().Name == typeName)
            {
                b.enabled = active;
                b.gameObject.SetActive(active);
            }
        }
    }

    static GameObject FindSceneObjectIncludingInactive(string objectName)
    {
        var scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
            return null;

        var roots = scene.GetRootGameObjects();
        for (var i = 0; i < roots.Length; i++)
        {
            var found = FindInHierarchy(roots[i].transform, objectName);
            if (found != null)
                return found.gameObject;
        }

        return null;
    }

    static Transform FindInHierarchy(Transform parent, string objectName)
    {
        if (parent.name == objectName)
            return parent;

        for (var i = 0; i < parent.childCount; i++)
        {
            var found = FindInHierarchy(parent.GetChild(i), objectName);
            if (found != null)
                return found;
        }

        return null;
    }
}

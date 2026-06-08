using System.IO;
using UnityEngine;

public class DataEnvironmentConfig : MonoBehaviour
{
    [SerializeField] DataEnvironment activeEnvironment = DataEnvironment.HarputProduction;

    public DataEnvironment ActiveEnvironment
    {
        get { return activeEnvironment; }
        set { activeEnvironment = value; }
    }

    public string GetEnvironmentFolderName()
    {
        switch (activeEnvironment)
        {
            case DataEnvironment.HarputProduction:
                return "HarputProduction";
            case DataEnvironment.FiratCampusTest:
                return "FiratCampusTest";
            case DataEnvironment.CurrentLocationTest:
                return "CurrentLocationTest";
            case DataEnvironment.ParkOutdoorTest:
                return "ParkOutdoorTest";
            case DataEnvironment.MesireAlani:
                return "MesireAlani";
            default:
                return "CurrentLocationTest";
        }
    }

    public bool ShowsOutdoorGpsNpcs()
    {
        return activeEnvironment == DataEnvironment.ParkOutdoorTest ||
            activeEnvironment == DataEnvironment.MesireAlani;
    }

    public string GetDataRootPath()
    {
        return Path.Combine(Application.streamingAssetsPath, "Data", GetEnvironmentFolderName()).Replace("\\", "/");
    }

    public bool UsesGpsRouteMode()
    {
        return activeEnvironment == DataEnvironment.ParkOutdoorTest ||
            activeEnvironment == DataEnvironment.MesireAlani ||
            activeEnvironment == DataEnvironment.HarputProduction ||
            activeEnvironment == DataEnvironment.FiratCampusTest ||
            activeEnvironment == DataEnvironment.CurrentLocationTest;
    }

    public bool UsesQrTriggerMode()
    {
        return activeEnvironment == DataEnvironment.HarputProduction;
    }

    public bool UsesQrLocationExperienceMvp()
    {
        return activeEnvironment == DataEnvironment.HarputProduction;
    }

    public void ApplyPlayMode()
    {
        gameObject.SetActive(true);

        if (UsesQrLocationExperienceMvp())
            ApplyQrLocationExperienceMvpMode();
        else if (UsesGpsRouteMode())
            ApplyGpsRouteMode();
    }

    void ApplyQrLocationExperienceMvpMode()
    {
        DataEnvironmentModeActivator.SetGpsRouteModeActive(false);

        DataEnvironmentModeActivator.SetComponentsActive<IndoorNpcTestManager>(false);
        DataEnvironmentModeActivator.SetObjectActive("IndoorNpcSetupPanel", false);
        DataEnvironmentModeActivator.SetObjectActive("IndoorNpcInteractionPanel", false);
        DataEnvironmentModeActivator.SetObjectActive("IndoorNpcRoot", false);

        DisableArTemplateDemoUi();

        DataEnvironmentModeActivator.SetComponentsActive<DataEnvironmentConfig>(true);
        DataEnvironmentModeActivator.SetComponentsActive<JsonDataLoader>(false);
        DataEnvironmentModeActivator.SetComponentsActive<LocationManager>(false);
        DataEnvironmentModeActivator.SetComponentsActive<LocationTriggerManager>(false);
        DataEnvironmentModeActivator.SetComponentsActive<QrLocationTriggerBridge>(false);
        DataEnvironmentModeActivator.SetComponentsActive<QrLocationNpcPresenter>(false);
        DataEnvironmentModeActivator.SetComponentsActive<QrFlowController>(false);
        DataEnvironmentModeActivator.SetComponentsActive<QuestProgressManager>(false);
        DataEnvironmentModeActivator.SetComponentsActive<QuestInteractionUI>(false);
        DataEnvironmentModeActivator.SetComponentsActive<QrCodeScanService>(false);
        DataEnvironmentModeActivator.SetComponentsActive<AppFlowController>(true);
        DataEnvironmentModeActivator.SetComponentsActive<ARModelViewer>(false);

        DataEnvironmentModeActivator.SetObjectActive("LocationDebugPanel", false);
        DataEnvironmentModeActivator.SetObjectActive("LocationTriggerDebugPanel", false);
        DataEnvironmentModeActivator.SetObjectActive("DataDebugPanel", false);
        DataEnvironmentModeActivator.SetObjectActive("QuestProgressDebugPanel", false);
        DataEnvironmentModeActivator.SetObjectActive("QuestInteractionPanel", false);
        DataEnvironmentModeActivator.SetObjectActive("QrFlowOverlay", false);

        LocationExperienceRuntimeSetup.EnsureFlowController();
        DataEnvironmentModeActivator.EnsureQrRouteUiVisible();
    }

    void ApplyGpsRouteMode()
    {
        QrLocationTriggerRuntimeSetup.EnsureComponents();

        if (UsesQrTriggerMode())
            QrFlowRuntimeSetup.EnsureFlowController();

        DataEnvironmentModeActivator.SetGpsRouteModeActive(true);

        DataEnvironmentModeActivator.SetComponentsActive<IndoorNpcTestManager>(false);
        DataEnvironmentModeActivator.SetObjectActive("IndoorNpcSetupPanel", false);
        DataEnvironmentModeActivator.SetObjectActive("IndoorNpcInteractionPanel", false);
        DataEnvironmentModeActivator.SetObjectActive("IndoorNpcRoot", false);

        DisableArTemplateDemoUi();

        DataEnvironmentModeActivator.SetComponentsActive<DataEnvironmentConfig>(true);
        DataEnvironmentModeActivator.SetComponentsActive<JsonDataLoader>(true);
        DataEnvironmentModeActivator.SetComponentsActive<LocationTriggerManager>(true);
        DataEnvironmentModeActivator.SetComponentsActive<QrLocationTriggerBridge>(true);
        DataEnvironmentModeActivator.SetComponentsActive<QrCodeScanService>(UsesQrTriggerMode());
        DataEnvironmentModeActivator.SetComponentsActive<QrLocationNpcPresenter>(UsesQrTriggerMode());
        DataEnvironmentModeActivator.SetComponentsActive<QrFlowController>(UsesQrTriggerMode());
        DataEnvironmentModeActivator.SetComponentsActive<QuestProgressManager>(true);
        DataEnvironmentModeActivator.SetComponentsActive<QuestInteractionUI>(true);

        if (UsesQrTriggerMode())
        {
            DataEnvironmentModeActivator.SetObjectActive("LocationDebugPanel", false);
            DataEnvironmentModeActivator.SetObjectActive("LocationTriggerDebugPanel", false);
            DataEnvironmentModeActivator.SetObjectActive("DataDebugPanel", false);
            DataEnvironmentModeActivator.SetObjectActive("QuestProgressDebugPanel", false);
            DataEnvironmentModeActivator.SetObjectActive("QuestInteractionPanel", false);
        }
        else
        {
            DataEnvironmentModeActivator.SetObjectActive("QuestInteractionPanel", true);
        }

        var triggers = UnityEngine.Object.FindObjectsByType<LocationTriggerManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < triggers.Length; i++)
        {
            var trigger = triggers[i];
            if (trigger == null)
                continue;

            trigger.SetTriggerSource(UsesQrTriggerMode() ? LocationTriggerSource.QrCode : LocationTriggerSource.GpsProximity);
        }

        var loaders = UnityEngine.Object.FindObjectsByType<JsonDataLoader>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < loaders.Length; i++)
        {
            var loader = loaders[i];
            if (loader == null)
                continue;

            if (!loader.IsLoaded)
                loader.ReloadData();
        }

        if (UsesQrTriggerMode())
        {
            QrFlowRuntimeSetup.EnsureFlowController();
            DataEnvironmentModeActivator.EnsureQrRouteUiVisible();

            var bridges = UnityEngine.Object.FindObjectsByType<QrLocationTriggerBridge>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < bridges.Length; i++)
            {
                if (bridges[i] != null)
                    bridges[i].BeginInitializeRoutine();
            }
        }
        else
        {
            DataEnvironmentModeActivator.EnsureGpsRouteDebugUiVisible();
        }
    }

    static void DisableArTemplateDemoUi()
    {
        var templateObjects = new[]
        {
            "Object Spawner",
            "Object Menu",
            "Object Menu Animator",
            "Create Button",
            "Delete Button",
            "Options Button",
            "Options Modal",
            "Remove Objects Button",
            "Cancel Button",
            "Hints Button",
            "Greeting Prompt",
            "Debug Plane Toggle",
            "Debug Menu Toggle",
            "Goal Manager",
            "Onboarding",
            "Goals"
        };

        for (var i = 0; i < templateObjects.Length; i++)
            DataEnvironmentModeActivator.SetObjectActive(templateObjects[i], false);

        DataEnvironmentModeActivator.SetComponentBehavioursActiveByTypeName("ARTemplateMenuManager", false);
        DataEnvironmentModeActivator.SetComponentBehavioursActiveByTypeName("GoalManager", false);
        DataEnvironmentModeActivator.SetComponentBehavioursActiveByTypeName("ObjectSpawner", false);
    }
}

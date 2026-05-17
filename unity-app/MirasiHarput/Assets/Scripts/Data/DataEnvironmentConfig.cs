using System.IO;
using UnityEngine;

public class DataEnvironmentConfig : MonoBehaviour
{
    [SerializeField] DataEnvironment activeEnvironment = DataEnvironment.CurrentLocationTest;

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

    public void ApplyPlayMode()
    {
        gameObject.SetActive(true);

        if (UsesGpsRouteMode())
            ApplyGpsRouteMode();
    }

    void ApplyGpsRouteMode()
    {
        DataEnvironmentModeActivator.SetGpsRouteModeActive(true);

        DataEnvironmentModeActivator.SetComponentsActive<IndoorNpcTestManager>(false);
        DataEnvironmentModeActivator.SetObjectActive("IndoorNpcSetupPanel", false);
        DataEnvironmentModeActivator.SetObjectActive("IndoorNpcInteractionPanel", false);
        DataEnvironmentModeActivator.SetObjectActive("IndoorNpcRoot", false);

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

        DataEnvironmentModeActivator.SetComponentsActive<DataEnvironmentConfig>(true);
        DataEnvironmentModeActivator.SetComponentsActive<JsonDataLoader>(true);
        DataEnvironmentModeActivator.SetComponentsActive<LocationTriggerManager>(true);
        DataEnvironmentModeActivator.SetComponentsActive<QuestProgressManager>(true);
        DataEnvironmentModeActivator.SetComponentsActive<QuestInteractionUI>(true);
        DataEnvironmentModeActivator.SetObjectActive("QuestInteractionPanel", true);

        var loaders = UnityEngine.Object.FindObjectsByType<JsonDataLoader>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < loaders.Length; i++)
        {
            var loader = loaders[i];
            if (loader == null)
                continue;

            if (!loader.IsLoaded)
                loader.ReloadData();
        }

        DataEnvironmentModeActivator.EnsureGpsRouteDebugUiVisible();
    }
}

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
            default:
                return "CurrentLocationTest";
        }
    }

    public string GetDataRootPath()
    {
        return Path.Combine(Application.streamingAssetsPath, "Data", GetEnvironmentFolderName()).Replace("\\", "/");
    }
}

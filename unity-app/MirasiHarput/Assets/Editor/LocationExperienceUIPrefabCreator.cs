#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class LocationExperienceUIPrefabCreator
{
    const string PrefabFolder = "Assets/Resources/UI/LocationExperience";
    const string PrefabPath = PrefabFolder + "/LocationExperienceUI.prefab";

    [MenuItem("Mirasi Harput/UI/Create Location Experience UI Prefab")]
    public static void CreatePrefab()
    {
        EnsureFolder(PrefabFolder);

        var root = LocationExperienceUICanvasFactory.BuildRoot();
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("[LocationExperienceUI] Prefab oluşturuldu: " + PrefabPath);
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/UI"))
            AssetDatabase.CreateFolder("Assets/Resources", "UI");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/UI/LocationExperience"))
            AssetDatabase.CreateFolder("Assets/Resources/UI", "LocationExperience");
    }
}
#endif

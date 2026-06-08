using UnityEngine;

public class ARModelViewer : MonoBehaviour
{
    [SerializeField] Camera arCamera = null;
    [SerializeField, Min(0.5f)] float anchorDistanceMeters = 2.2f;

    Transform modelRoot;
    GameObject activeModel;
    string activeLocationId = string.Empty;

    public bool HasActiveModel
    {
        get { return activeModel != null; }
    }

    void Awake()
    {
        EnsureModelRoot();
    }

    void LateUpdate()
    {
        if (activeModel == null)
            return;

        UpdateAnchorPose();
    }

    void EnsureModelRoot()
    {
        if (modelRoot != null)
            return;

        var rootGo = new GameObject("LocationModelAnchor");
        modelRoot = rootGo.transform;
        modelRoot.SetParent(transform, false);
    }

    void ResolveCamera()
    {
        if (arCamera != null)
            return;

        arCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
    }

    void UpdateAnchorPose()
    {
        ResolveCamera();
        if (arCamera == null || modelRoot == null)
            return;

        var cam = arCamera.transform;
        var forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = cam.forward;

        forward.Normalize();
        modelRoot.position = cam.position + forward * anchorDistanceMeters;
        modelRoot.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    public bool TryShowLocation(LocationModel location)
    {
        if (location == null)
            return false;

        if (activeModel != null && activeLocationId == location.id)
            return true;

        ClearModel();
        EnsureModelRoot();

        var instance = TryInstantiateModel(location);
        if (instance == null)
            instance = CreateFallbackModel(location.title);

        instance.transform.SetParent(modelRoot, false);
        ApplyTransform(instance.transform, location);
        activeModel = instance;
        activeLocationId = location.id;
        UpdateAnchorPose();
        return true;
    }

    static GameObject TryInstantiateModel(LocationModel location)
    {
        if (location == null || string.IsNullOrEmpty(location.modelPath))
            return null;

        var prefab = Resources.Load<GameObject>(location.modelPath);
        if (prefab == null)
            return null;

        return Instantiate(prefab);
    }

    static void ApplyTransform(Transform target, LocationModel location)
    {
        if (target == null || location == null)
            return;

        var position = location.modelPosition != null ? location.modelPosition.ToVector3() : Vector3.zero;
        var rotation = location.modelRotation != null ? location.modelRotation.ToVector3() : Vector3.zero;
        target.localPosition = position;
        target.localRotation = Quaternion.Euler(rotation);
        target.localScale = Vector3.one * Mathf.Max(0.01f, location.modelScale);
    }

    static GameObject CreateFallbackModel(string title)
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        root.name = "ModelPlaceholder";

        var renderer = root.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = new Color(0.72f, 0.72f, 0.78f, 1f);

        var collider = root.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(root.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        labelGo.transform.localScale = Vector3.one * 0.08f;

        var textMesh = labelGo.AddComponent<TextMesh>();
        textMesh.text = string.IsNullOrEmpty(title) ? "3D Model" : title;
        textMesh.fontSize = 48;
        textMesh.characterSize = 0.12f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;

        return root;
    }

    public void ClearModel()
    {
        if (activeModel != null)
            Destroy(activeModel);

        activeModel = null;
        activeLocationId = string.Empty;
    }
}

using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class NpcArBillboardVisual : MonoBehaviour
{
    static Mesh sharedQuadMesh;
    static Shader cachedShader;

    [SerializeField, Min(0.5f)] float heightMeters = 1.55f;
    [SerializeField] bool faceCamera = true;
    [SerializeField] bool addCollider = true;
    [SerializeField] float alphaCutoff = 0.01f;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    BoxCollider boxCollider;
    Material materialInstance;
    Texture2D runtimeTexture;
    Camera targetCamera;

    public Renderer Renderer => meshRenderer;

    public void SetFaceCameraEnabled(bool enabled)
    {
        faceCamera = enabled;
    }

    public static GameObject Create(string npcName, Texture2D texture, float npcHeightMeters, Transform parent, string displayTitle = null)
    {
        var root = new GameObject(npcName);
        if (parent != null)
            root.transform.SetParent(parent, false);

        var visual = root.AddComponent<NpcArBillboardVisual>();
        visual.heightMeters = npcHeightMeters;
        visual.BuildFromTexture(texture);

        if (!string.IsNullOrWhiteSpace(displayTitle))
            NpcVisualSpawn.AddTitleLabel(visual.transform, displayTitle, npcHeightMeters + 0.14f);

        return root;
    }

    public void BuildFromTexture(Texture2D texture)
    {
        EnsureComponents();

        var aspect = texture != null && texture.height > 0
            ? (float)texture.width / texture.height
            : 0.45f;
        var width = heightMeters * aspect;
        transform.localScale = new Vector3(width, heightMeters, 1f);

        if (materialInstance != null)
            Destroy(materialInstance);
        if (runtimeTexture != null)
        {
            Destroy(runtimeTexture);
            runtimeTexture = null;
        }

        var preparedTexture = NpcPngTextureUtility.PrepareForBillboard(texture);
        if (preparedTexture != null && preparedTexture != texture)
            runtimeTexture = preparedTexture;

        materialInstance = CreatePngMaterial(preparedTexture, alphaCutoff);
        meshRenderer.sharedMaterial = materialInstance;
        meshRenderer.enabled = true;

        if (addCollider)
            EnsureCollider();
    }

    void Awake()
    {
        EnsureComponents();
    }

    void LateUpdate()
    {
        if (!faceCamera)
            return;

        ResolveCamera();
        if (targetCamera == null)
            return;

        var toCamera = targetCamera.transform.position - transform.position;
        if (toCamera.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
    }

    void OnDestroy()
    {
        if (materialInstance != null)
            Destroy(materialInstance);
        if (runtimeTexture != null)
            Destroy(runtimeTexture);
    }

    void EnsureComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (sharedQuadMesh == null)
            sharedQuadMesh = BuildQuadMesh();
        meshFilter.sharedMesh = sharedQuadMesh;

        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }

    void EnsureCollider()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider>();

        boxCollider.center = new Vector3(0f, 0.5f, 0f);
        boxCollider.size = new Vector3(1f, 1f, 0.05f);
    }

    void ResolveCamera()
    {
        if (targetCamera == null)
            targetCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
    }

    static Mesh BuildQuadMesh()
    {
        var mesh = new Mesh { name = "NpcBillboardQuad" };
        mesh.vertices = new[]
        {
            new Vector3(-0.5f, 0f, 0f),
            new Vector3(0.5f, 0f, 0f),
            new Vector3(0.5f, 1f, 0f),
            new Vector3(-0.5f, 1f, 0f)
        };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f)
        };
        mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    static Material CreatePngMaterial(Texture2D texture, float cutoff)
    {
        var shader = ResolveShader();
        var material = new Material(shader);
        if (texture != null)
        {
            material.mainTexture = texture;
            if (material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex"))
                material.SetTexture("_MainTex", texture);
        }

        if (material.HasProperty("_Cutoff"))
            material.SetFloat("_Cutoff", cutoff);

        material.color = Color.white;
        material.renderQueue = (int)RenderQueue.Transparent;
        return material;
    }

    static Shader ResolveShader()
    {
        if (cachedShader != null)
            return cachedShader;

        cachedShader = Resources.Load<Shader>("NpcBillboard");
        if (cachedShader != null)
            return cachedShader;

        cachedShader = Shader.Find("MirasiHarput/NpcBillboard");
        if (cachedShader != null)
            return cachedShader;

        cachedShader = Shader.Find("Sprites/Default");
        return cachedShader;
    }
}

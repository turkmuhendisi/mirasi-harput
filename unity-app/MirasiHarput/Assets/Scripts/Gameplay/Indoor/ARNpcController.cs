using UnityEngine;

public class ARNpcController : MonoBehaviour
{
    [SerializeField] string npcId = string.Empty;
    [SerializeField] string npcName = string.Empty;
    [SerializeField] bool isCurrentTarget;
    [SerializeField] bool canInteract = true;
    [SerializeField] bool faceCamera = true;

    IndoorNpcTestManager manager;
    Camera targetCamera;

    public string NpcId => npcId;
    public string NpcName => npcName;
    public bool IsCurrentTarget => isCurrentTarget;
    public bool CanInteract => canInteract;

    void Awake()
    {
        EnsureCollider();
        ResolveCamera();
    }

    void Update()
    {
        if (!faceCamera)
            return;

        ResolveCamera();
        if (targetCamera == null)
            return;

        var direction = transform.position - targetCamera.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    public void Setup(string id, string name, bool isTarget)
    {
        npcId = id;
        npcName = name;
        SetAsTarget(isTarget);
        EnsureCollider();
    }

    public void SetManager(IndoorNpcTestManager testManager)
    {
        manager = testManager;
    }

    public void SetAsTarget(bool value)
    {
        isCurrentTarget = value;
        canInteract = value;
    }

    public void SetFaceCamera(bool value)
    {
        faceCamera = value;
    }

    public void OnTapped()
    {
        if (!canInteract || manager == null || string.IsNullOrEmpty(npcId))
            return;

        manager.HandleNpcInteraction(npcId);
    }

    void EnsureCollider()
    {
        if (GetComponent<Collider>() == null)
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(0.75f, 1.6f, 0.08f);
            box.center = new Vector3(0f, 0.8f, 0f);
        }
    }

    void ResolveCamera()
    {
        if (targetCamera == null)
            targetCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
    }
}

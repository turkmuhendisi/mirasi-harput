using System;
using System.IO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class IndoorNpcTestManager : MonoBehaviour
{
    const string SaveFileName = "indoor_npc_test_anchors.json";
    const string Npc1ResourcePath = "IndoorNpc/npc_guide_1";
    const string Npc2ResourcePath = "IndoorNpc/npc_guide_2";

    [Header("State")]
    [SerializeField] bool setupCompleted;
    [SerializeField] string firstNpcId = "indoor_npc_1";
    [SerializeField] string secondNpcId = "indoor_npc_2";
    [SerializeField] string firstNpcName = "Rehber 1";
    [SerializeField] string secondNpcName = "Rehber 2";
    [SerializeField] bool firstNpcInteracted;
    [SerializeField] bool secondNpcInteracted;
    [SerializeField] int rewardScore = 20;
    [SerializeField] string rewardBadgeId = "badge_indoor_npc_test";
    [SerializeField] string statusMessage = "Indoor NPC test bekleniyor";

    [Header("References")]
    [SerializeField] IndoorNpcPlacementController placementController = null;
    [SerializeField] IndoorNpcSetupUI setupUI = null;
    [SerializeField] IndoorNpcInteractionUI interactionUI = null;
    [SerializeField] QuestProgressManager questProgressManager = null;
    [SerializeField] ARRaycastManager raycastManager = null;
    [SerializeField] Camera arCamera = null;
    [SerializeField] GameObject npc1Prefab = null;
    [SerializeField] GameObject npc2Prefab = null;
    [SerializeField] Transform indoorNpcRoot = null;

    [Header("Options")]
    [SerializeField] bool initializeOnStart = true;
    [SerializeField] bool disableTemplateDemoObjects = true;
    [SerializeField] float fallbackNpcHeightMeters = 1.55f;
    [SerializeField, Min(16f)] float npcScreenTapRadiusPixels = 180f;

    IndoorNpcAnchorData firstNpcAnchor = new IndoorNpcAnchorData();
    IndoorNpcAnchorData secondNpcAnchor = new IndoorNpcAnchorData();
    ARNpcController firstNpcController;
    ARNpcController secondNpcController;

    public string FirstNpcId => firstNpcId;
    public string SecondNpcId => secondNpcId;
    public string FirstNpcName => firstNpcName;
    public string SecondNpcName => secondNpcName;
    public bool SetupCompleted => setupCompleted;
    public bool IsFirstNpcPlaced => firstNpcAnchor != null && firstNpcAnchor.isPlaced;
    public bool IsSecondNpcPlaced => secondNpcAnchor != null && secondNpcAnchor.isPlaced;
    public bool FirstNpcInteracted => firstNpcInteracted;
    public bool SecondNpcInteracted => secondNpcInteracted;
    public int RewardScore => rewardScore;
    public string RewardBadgeId => rewardBadgeId;
    public string StatusMessage => statusMessage;

    string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    void Awake()
    {
        ResolveReferences();
        EnsureAnchorDefaults();

        if (disableTemplateDemoObjects)
            DisableTemplateDemoObjects();
    }

    void Start()
    {
        if (initializeOnStart)
            InitializeIndoorTest();
    }

    void Update()
    {
        HandleNpcTapInput();
    }

    public void InitializeIndoorTest()
    {
        ResolveReferences();
        EnsureAnchorDefaults();

        firstNpcInteracted = false;
        secondNpcInteracted = false;

        if (LoadSavedAnchors())
        {
            setupCompleted = true;
            SpawnNpcFromSavedData();
            SetCurrentTarget(firstNpcId);
            SetStatusMessage("Indoor NPC test hazır. İlk hedef: " + firstNpcName);
            if (setupUI != null)
                setupUI.Hide();
            return;
        }

        setupCompleted = false;
        StartSetupMode();
    }

    public bool LoadSavedAnchors()
    {
        EnsureAnchorDefaults();

        if (!File.Exists(SavePath))
            return false;

        try
        {
            var json = File.ReadAllText(SavePath);
            var saveData = JsonUtility.FromJson<IndoorNpcAnchorSaveData>(json);
            if (saveData == null || saveData.firstNpc == null || saveData.secondNpc == null)
                return false;

            firstNpcAnchor = saveData.firstNpc;
            secondNpcAnchor = saveData.secondNpc;
            setupCompleted = saveData.setupCompleted && firstNpcAnchor.isPlaced && secondNpcAnchor.isPlaced;
            return setupCompleted;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Indoor NPC anchor verisi okunamadı: " + exception.Message);
            return false;
        }
    }

    public void SaveAnchors()
    {
        EnsureAnchorDefaults();

        var saveData = new IndoorNpcAnchorSaveData
        {
            setupCompleted = setupCompleted,
            firstNpc = firstNpcAnchor,
            secondNpc = secondNpcAnchor
        };

        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(saveData, true));
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Indoor NPC anchor verisi kaydedilemedi: " + exception.Message);
        }
    }

    public void StartSetupMode()
    {
        ResolveReferences();
        setupCompleted = false;

        if (setupUI != null)
        {
            setupUI.SetManager(this);
            setupUI.Show();
            setupUI.SetFinishInteractable(IsFirstNpcPlaced && IsSecondNpcPlaced);
        }

        if (!IsFirstNpcPlaced)
            SetStatusMessage("1. odada NPC-1'i yerleştirin.");
        else if (!IsSecondNpcPlaced)
            SetStatusMessage("2. odada NPC-2'yi yerleştirin.");
        else
            SetStatusMessage("İki NPC de yerleştirildi. Kurulumu bitirebilirsin.");

        if (placementController != null)
            placementController.BeginPlacement();
    }

    public void CompleteSetupMode()
    {
        if (!IsFirstNpcPlaced || !IsSecondNpcPlaced)
        {
            SetStatusMessage("Kurulum tamamlanmadı. Önce iki NPC'yi de yerleştir.");
            return;
        }

        setupCompleted = true;
        firstNpcInteracted = false;
        secondNpcInteracted = false;
        SaveAnchors();
        SpawnNpcFromSavedData();
        SetCurrentTarget(firstNpcId);

        if (setupUI != null)
            setupUI.Hide();

        if (interactionUI != null)
            interactionUI.Hide();

        SetStatusMessage("Indoor NPC setup tamamlandı. İlk hedef: " + firstNpcName);
    }

    public void SpawnNpcFromSavedData()
    {
        ResolveReferences();
        EnsureAnchorDefaults();

        DestroySpawnedNpcs();

        if (firstNpcAnchor.isPlaced)
            firstNpcController = SpawnNpc(firstNpcAnchor, npc1Prefab, Npc1ResourcePath, firstNpcId == GetCurrentTargetId());

        if (secondNpcAnchor.isPlaced)
            secondNpcController = SpawnNpc(secondNpcAnchor, npc2Prefab, Npc2ResourcePath, secondNpcId == GetCurrentTargetId());
    }

    public void RegisterNpcPlacement(string npcId, string npcName, Pose worldPose)
    {
        ResolveReferences();
        EnsureAnchorDefaults();

        var anchorData = npcId == firstNpcId ? firstNpcAnchor : secondNpcAnchor;
        anchorData.npcId = npcId;
        anchorData.npcName = npcName;
        anchorData.localPosition = indoorNpcRoot != null ? indoorNpcRoot.InverseTransformPoint(worldPose.position) : worldPose.position;
        anchorData.localRotation = indoorNpcRoot != null ? Quaternion.Inverse(indoorNpcRoot.rotation) * worldPose.rotation : worldPose.rotation;
        anchorData.isPlaced = true;

        SpawnNpcFromSavedData();

        if (setupUI != null)
            setupUI.SetFinishInteractable(IsFirstNpcPlaced && IsSecondNpcPlaced);

        if (!IsSecondNpcPlaced)
        {
            SetStatusMessage("NPC-1 yerleştirildi. Şimdi 2. odaya geçip NPC-2'yi yerleştir.");
            return;
        }

        SetStatusMessage("NPC-2 yerleştirildi. Kurulum tamamlanıyor.");
        CompleteSetupMode();
    }

    public void HandleNpcInteraction(string npcId)
    {
        if (!setupCompleted)
        {
            SetStatusMessage("Önce indoor NPC setup tamamlanmalı.");
            return;
        }

        if (npcId == firstNpcId)
        {
            firstNpcInteracted = true;
            SetCurrentTarget(secondNpcId);
            SetStatusMessage("NPC-1 ile konuşuldu. Yeni hedef: " + secondNpcName);

            if (interactionUI != null)
            {
                interactionUI.ShowNpcDialogue(
                    firstNpcName,
                    "Merhaba. Test görevine hoş geldin. Şimdi diğer odadaki rehberle konuşmanı istiyorum.",
                    "Hedef: Diğer odadaki NPC ile konuş.");
            }

            return;
        }

        if (npcId == secondNpcId)
        {
            if (!firstNpcInteracted)
            {
                SetCurrentTarget(firstNpcId);
                SetStatusMessage("Önce NPC-1 ile konuşmalısın.");
                if (interactionUI != null)
                    interactionUI.ShowNpcDialogue(secondNpcName, "Önce diğer odadaki ilk rehberle konuşmalısın.", "Hedef: NPC-1 ile konuş.");
                return;
            }

            secondNpcInteracted = true;
            SetCurrentTarget(string.Empty);
            SetStatusMessage("NPC-2 ile konuşuldu. Indoor test görevi tamamlandı.");

            if (interactionUI != null)
            {
                interactionUI.ShowNpcDialogue(
                    secondNpcName,
                    "Tebrikler. İlk NPC'nin yönlendirmesini takip ederek ikinci NPC'ye ulaştın. Indoor test görevi tamamlandı.",
                    "Hedef: Indoor NPC test görevi tamamlandı.");
            }

            CompleteIndoorTest();
        }
    }

    public void CompleteIndoorTest()
    {
        secondNpcInteracted = true;
        SetCurrentTarget(string.Empty);
        SetStatusMessage("Indoor NPC test tamamlandı. Puan: " + rewardScore + ", rozet: " + rewardBadgeId);

        if (interactionUI != null)
        {
            interactionUI.ShowResult(
                rewardScore,
                rewardBadgeId,
                "İkinci NPC ile etkileşim kuruldu ve oda içi görev akışı tamamlandı.");
        }
    }

    public void ResetIndoorTest()
    {
        ClearSavedAnchors();
        firstNpcInteracted = false;
        secondNpcInteracted = false;
        setupCompleted = false;
        DestroySpawnedNpcs();

        if (placementController != null)
            placementController.ResetPlacement();

        if (interactionUI != null)
            interactionUI.Hide();

        StartSetupMode();
    }

    public void ClearSavedAnchors()
    {
        EnsureAnchorDefaults();
        firstNpcAnchor.isPlaced = false;
        secondNpcAnchor.isPlaced = false;
        setupCompleted = false;

        if (File.Exists(SavePath))
            File.Delete(SavePath);

        SetStatusMessage("Indoor NPC kayıtlı yerleşimleri temizlendi.");
    }

    public void SetStatusMessage(string message)
    {
        statusMessage = string.IsNullOrEmpty(message) ? "-" : message;

        if (setupUI != null)
            setupUI.SetStatus(statusMessage);
    }

    void ResolveReferences()
    {
        if (placementController == null)
            placementController = GetComponent<IndoorNpcPlacementController>() != null ? GetComponent<IndoorNpcPlacementController>() : FindAnyObjectByType<IndoorNpcPlacementController>();

        if (setupUI == null)
            setupUI = FindAnyObjectByType<IndoorNpcSetupUI>(FindObjectsInactive.Include);

        if (interactionUI == null)
            interactionUI = FindAnyObjectByType<IndoorNpcInteractionUI>(FindObjectsInactive.Include);

        if (questProgressManager == null)
            questProgressManager = FindAnyObjectByType<QuestProgressManager>();

        if (raycastManager == null)
            raycastManager = FindAnyObjectByType<ARRaycastManager>();

        if (arCamera == null)
            arCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();

        if (indoorNpcRoot == null)
        {
            var rootObject = GameObject.Find("IndoorNpcRoot");
            if (rootObject == null)
                rootObject = new GameObject("IndoorNpcRoot");

            indoorNpcRoot = rootObject.transform;
        }

        if (setupUI != null)
            setupUI.SetManager(this);
    }

    void EnsureAnchorDefaults()
    {
        if (firstNpcAnchor == null)
            firstNpcAnchor = new IndoorNpcAnchorData();

        if (secondNpcAnchor == null)
            secondNpcAnchor = new IndoorNpcAnchorData();

        if (string.IsNullOrEmpty(firstNpcAnchor.npcId))
            firstNpcAnchor.npcId = firstNpcId;

        if (string.IsNullOrEmpty(firstNpcAnchor.npcName))
            firstNpcAnchor.npcName = firstNpcName;

        if (string.IsNullOrEmpty(secondNpcAnchor.npcId))
            secondNpcAnchor.npcId = secondNpcId;

        if (string.IsNullOrEmpty(secondNpcAnchor.npcName))
            secondNpcAnchor.npcName = secondNpcName;
    }

    ARNpcController SpawnNpc(IndoorNpcAnchorData anchorData, GameObject prefab, string resourcePath, bool isTarget)
    {
        var npcObject = prefab != null ? Instantiate(prefab) : CreateFallbackNpc(anchorData.npcName, resourcePath);
        npcObject.name = anchorData.npcName;
        npcObject.transform.SetParent(indoorNpcRoot, false);
        npcObject.transform.localPosition = anchorData.localPosition;
        npcObject.transform.localRotation = anchorData.localRotation;

        var controller = npcObject.GetComponent<ARNpcController>();
        if (controller == null)
            controller = npcObject.AddComponent<ARNpcController>();

        controller.SetManager(this);
        controller.Setup(anchorData.npcId, anchorData.npcName, isTarget);
        return controller;
    }

    GameObject CreateFallbackNpc(string npcName, string resourcePath)
    {
        var texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.localScale = new Vector3(0.45f, fallbackNpcHeightMeters * 0.5f, 0.45f);
            AddNameLabel(capsule.transform, npcName, fallbackNpcHeightMeters + 0.18f);
            return capsule;
        }

        var npcObject = new GameObject(npcName);
        var renderer = npcObject.AddComponent<SpriteRenderer>();
        var pixelsPerUnit = texture.height / Mathf.Max(0.1f, fallbackNpcHeightMeters);
        renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0f), pixelsPerUnit);
        renderer.sortingOrder = 20;

        var collider = npcObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.8f, fallbackNpcHeightMeters, 0.08f);
        collider.center = new Vector3(0f, fallbackNpcHeightMeters * 0.5f, 0f);

        AddNameLabel(npcObject.transform, npcName, fallbackNpcHeightMeters + 0.16f);
        return npcObject;
    }

    void AddNameLabel(Transform parent, string label, float height)
    {
        var labelObject = new GameObject("NameLabel");
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = new Vector3(0f, height, 0f);

        var textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.fontSize = 42;
        textMesh.characterSize = 0.025f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
    }

    void DestroySpawnedNpcs()
    {
        if (firstNpcController != null)
            Destroy(firstNpcController.gameObject);

        if (secondNpcController != null)
            Destroy(secondNpcController.gameObject);

        firstNpcController = null;
        secondNpcController = null;
    }

    void SetCurrentTarget(string npcId)
    {
        if (firstNpcController != null)
            firstNpcController.SetAsTarget(npcId == firstNpcId);

        if (secondNpcController != null)
            secondNpcController.SetAsTarget(npcId == secondNpcId);
    }

    string GetCurrentTargetId()
    {
        if (!firstNpcInteracted)
            return firstNpcId;

        if (!secondNpcInteracted)
            return secondNpcId;

        return string.Empty;
    }

    void DisableTemplateDemoObjects()
    {
        var namesToDisable = new[]
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
            "Debug Menu Toggle"
        };

        for (var i = 0; i < namesToDisable.Length; i++)
        {
            var objectToDisable = GameObject.Find(namesToDisable[i]);
            if (objectToDisable != null)
                objectToDisable.SetActive(false);
        }
    }

    void HandleNpcTapInput()
    {
        if (!setupCompleted)
            return;

        if (interactionUI != null && interactionUI.IsVisible)
            return;

        if (!TryGetTapPosition(out var tapPosition))
            return;

        ResolveReferences();
        if (arCamera == null)
            return;

        var ray = arCamera.ScreenPointToRay(tapPosition);
        if (Physics.Raycast(ray, out var hit, 30f))
        {
            var npc = hit.collider.GetComponentInParent<ARNpcController>();
            if (npc != null)
            {
                npc.OnTapped();
                return;
            }
        }

        if (TryTapNearestProjectedNpc(tapPosition))
            return;

        TryTapCurrentTargetNpc();
    }

    bool TryTapNearestProjectedNpc(Vector2 tapPosition)
    {
        ResolveReferences();
        if (arCamera == null)
            return false;

        var npcs = FindObjectsByType<ARNpcController>(FindObjectsInactive.Exclude);
        ARNpcController closestNpc = null;
        var closestDistance = float.MaxValue;

        for (var i = 0; i < npcs.Length; i++)
        {
            var npc = npcs[i];
            if (npc == null || !npc.CanInteract)
                continue;

            var screenPoint = arCamera.WorldToScreenPoint(npc.transform.position + Vector3.up * 0.8f);
            if (screenPoint.z < 0f)
                continue;

            var distance = Vector2.Distance(tapPosition, new Vector2(screenPoint.x, screenPoint.y));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNpc = npc;
            }
        }

        if (closestNpc == null || closestDistance > npcScreenTapRadiusPixels)
            return false;

        closestNpc.OnTapped();
        return true;
    }

    bool TryTapCurrentTargetNpc()
    {
        if (firstNpcController != null && firstNpcController.CanInteract)
        {
            firstNpcController.OnTapped();
            return true;
        }

        if (secondNpcController != null && secondNpcController.CanInteract)
        {
            secondNpcController.OnTapped();
            return true;
        }

        return false;
    }

    bool TryGetTapPosition(out Vector2 tapPosition)
    {
#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            tapPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            tapPosition = Mouse.current.position.ReadValue();
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                tapPosition = touch.position;
                return true;
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            tapPosition = Input.mousePosition;
            return true;
        }
#endif
#endif

        tapPosition = Vector2.zero;
        return false;
    }

    [Serializable]
    class IndoorNpcAnchorSaveData
    {
        public bool setupCompleted;
        public IndoorNpcAnchorData firstNpc;
        public IndoorNpcAnchorData secondNpc;
    }
}

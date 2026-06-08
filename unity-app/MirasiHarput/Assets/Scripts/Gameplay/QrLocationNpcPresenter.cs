using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[DefaultExecutionOrder(25)]
public class QrLocationNpcPresenter : MonoBehaviour
{
    [SerializeField] JsonDataLoader dataLoader = null;
    [SerializeField] LocationTriggerManager locationTriggerManager = null;
    [SerializeField] ARRaycastManager raycastManager = null;
    [SerializeField] Camera arCamera = null;

    [Header("NPC")]
    [SerializeField, Min(0.1f)] float fallbackNpcHeightMeters = 1.55f;
    [SerializeField, Min(0.5f)] float preferredPlacementDistanceMeters = 2.2f;
    [SerializeField] float npcGroundClearanceY = 0.02f;

    Transform npcRoot;
    GameObject activeNpcObject;
    string activeLocationId = string.Empty;
    bool subscribed;

    void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
        ClearActiveNpc();
    }

    void ResolveReferences()
    {
        if (dataLoader == null)
            dataLoader = JsonDataLoader.Instance != null ? JsonDataLoader.Instance : FindAnyObjectByType<JsonDataLoader>();

        if (locationTriggerManager == null)
            locationTriggerManager = FindAnyObjectByType<LocationTriggerManager>();

        if (raycastManager == null)
            raycastManager = FindAnyObjectByType<ARRaycastManager>(FindObjectsInactive.Include);

        if (arCamera == null)
            arCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
    }

    void Subscribe()
    {
        if (subscribed || locationTriggerManager == null)
            return;

        locationTriggerManager.OnLocationTriggered -= HandleLocationTriggered;
        locationTriggerManager.OnLocationTriggered += HandleLocationTriggered;
        subscribed = true;
    }

    void Unsubscribe()
    {
        if (!subscribed || locationTriggerManager == null)
            return;

        locationTriggerManager.OnLocationTriggered -= HandleLocationTriggered;
        subscribed = false;
    }

    void HandleLocationTriggered(LocationData location, QuestData _)
    {
        if (location == null || locationTriggerManager == null || !locationTriggerManager.UsesQrTriggerMode)
            return;

        if (ShouldDeferToOutdoorGpsNpcView())
            return;

        PresentNpcForLocation(location);
    }

    bool ShouldDeferToOutdoorGpsNpcView()
    {
        var outdoor = FindAnyObjectByType<OutdoorGpsNpcViewManager>(FindObjectsInactive.Include);
        return outdoor != null && outdoor.isActiveAndEnabled;
    }

    void PresentNpcForLocation(LocationData location)
    {
        if (location == null || string.IsNullOrEmpty(location.id))
            return;

        ResolveReferences();

        if (activeNpcObject != null && activeLocationId == location.id)
        {
            PlaceOnGround(activeNpcObject.transform);
            return;
        }

        ClearActiveNpc();
        EnsureNpcRoot();

        var resourcePath = ResolveNpcResourcePath(location);
        var displayTitle = ResolveNpcDisplayTitle(location);
        var go = NpcVisualSpawn.CreateOutdoorAr(location.name, resourcePath, fallbackNpcHeightMeters, displayTitle);

        go.name = "QrNpc_" + location.id;
        go.transform.SetParent(npcRoot, false);

        var capsuleCol = go.GetComponent<CapsuleCollider>();
        if (capsuleCol != null)
            Destroy(capsuleCol);

        var controller = go.GetComponent<ARNpcController>();
        if (controller == null)
            controller = go.AddComponent<ARNpcController>();
        controller.SetManager(null);
        controller.Setup(location.npcId, location.name, true);
        controller.SetFaceCamera(false);

        var billboard = go.GetComponent<NpcArBillboardVisual>();
        if (billboard != null)
            billboard.SetFaceCameraEnabled(true);

        PlaceOnGround(go.transform);
        activeNpcObject = go;
        activeLocationId = location.id;
    }

    void PlaceOnGround(Transform npcTransform)
    {
        if (npcTransform == null)
            return;

        if (ArNpcGroundPlacer.TryGetGroundPoseForNpc(
            arCamera,
            raycastManager,
            fallbackNpcHeightMeters,
            preferredPlacementDistanceMeters,
            out var npcPose))
        {
            npcTransform.SetPositionAndRotation(npcPose.position, npcPose.rotation);
            return;
        }

        var cam = arCamera != null ? arCamera : Camera.main;
        if (cam == null)
            return;

        var forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.forward;

        forward.Normalize();
        var fallbackGround = cam.transform.position + forward * preferredPlacementDistanceMeters;
        fallbackGround.y = cam.transform.position.y - 1.45f + npcGroundClearanceY;
        npcTransform.position = fallbackGround + Vector3.up * (fallbackNpcHeightMeters * 0.5f);
        npcTransform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    void EnsureNpcRoot()
    {
        if (npcRoot != null)
            return;

        var rootGo = new GameObject("QrLocationNpcs");
        npcRoot = rootGo.transform;
    }

    public void ClearActiveNpc()
    {
        if (activeNpcObject != null)
            Destroy(activeNpcObject);

        activeNpcObject = null;
        activeLocationId = string.Empty;
    }

    static string ResolveNpcResourcePath(LocationData loc)
    {
        if (loc == null)
            return IndoorNpcTestManager.Npc1ResourcePath;

        if (!string.IsNullOrEmpty(loc.imageKey) &&
            loc.imageKey.StartsWith("npc_guide", StringComparison.OrdinalIgnoreCase))
            return "ParkNpc/" + loc.imageKey;

        return UsesSecondGuideCharacter(loc.npcId)
            ? IndoorNpcTestManager.Npc2ResourcePath
            : IndoorNpcTestManager.Npc1ResourcePath;
    }

    static string ResolveNpcDisplayTitle(LocationData loc)
    {
        if (loc == null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(loc.npcDisplayTitle))
            return loc.npcDisplayTitle.Trim();

        return loc.name;
    }

    static bool UsesSecondGuideCharacter(string npcId)
    {
        if (string.IsNullOrEmpty(npcId))
            return false;

        return npcId.IndexOf("sarahatun", StringComparison.OrdinalIgnoreCase) >= 0 ||
            npcId.IndexOf("arap_baba", StringComparison.OrdinalIgnoreCase) >= 0 ||
            npcId.IndexOf("final", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

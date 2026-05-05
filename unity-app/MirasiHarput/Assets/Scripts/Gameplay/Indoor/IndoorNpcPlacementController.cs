using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class IndoorNpcPlacementController : MonoBehaviour
{
    enum PlacementStep
    {
        None,
        FirstNpc,
        SecondNpc
    }

    [SerializeField] IndoorNpcTestManager manager = null;
    [SerializeField] ARRaycastManager raycastManager = null;
    [SerializeField] Camera arCamera = null;
    [SerializeField] bool allowCameraForwardFallback = true;
    [SerializeField] float editorFallbackDistanceMeters = 1.5f;
    [SerializeField] float fallbackVerticalOffsetMeters = -1.1f;

    static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();

    PlacementStep currentStep = PlacementStep.None;
    bool placementActive;

    public bool IsPlacementActive => placementActive;

    void Awake()
    {
        ResolveReferences();
    }

    void Update()
    {
        if (!placementActive)
            return;

        if (!TryGetTapPosition(out var tapPosition))
            return;

        if (IsPointerOverUI(tapPosition))
            return;

        if (!TryGetPlacementPose(tapPosition, out var placementPose))
        {
            if (manager != null)
                manager.SetStatusMessage("AR yüzeyi bulunamadı. Kamerayı zemine doğru yavaşça gezdir.");
            return;
        }

        if (currentStep == PlacementStep.FirstNpc)
            PlaceFirstNpc(placementPose);
        else if (currentStep == PlacementStep.SecondNpc)
            PlaceSecondNpc(placementPose);
    }

    public void BeginPlacement()
    {
        ResolveReferences();

        if (manager == null)
            return;

        if (!manager.IsFirstNpcPlaced)
        {
            currentStep = PlacementStep.FirstNpc;
            placementActive = true;
            manager.SetStatusMessage("1. odada NPC-1'i yerleştirmek için algılanan yüzeye dokun.");
            TryPlaceAtScreenCenter();
            return;
        }

        if (!manager.IsSecondNpcPlaced)
        {
            currentStep = PlacementStep.SecondNpc;
            placementActive = true;
            manager.SetStatusMessage("2. odada NPC-2'yi yerleştirmek için algılanan yüzeye dokun.");
            TryPlaceAtScreenCenter();
            return;
        }

        placementActive = false;
        currentStep = PlacementStep.None;
        manager.CompleteSetupMode();
    }

    public void PlaceFirstNpc(Pose pose)
    {
        ResolveReferences();
        placementActive = false;
        currentStep = PlacementStep.None;

        if (manager != null)
            manager.RegisterNpcPlacement(manager.FirstNpcId, manager.FirstNpcName, pose);
    }

    public void PlaceSecondNpc(Pose pose)
    {
        ResolveReferences();
        placementActive = false;
        currentStep = PlacementStep.None;

        if (manager != null)
            manager.RegisterNpcPlacement(manager.SecondNpcId, manager.SecondNpcName, pose);
    }

    public void ResetPlacement()
    {
        placementActive = false;
        currentStep = PlacementStep.None;
    }

    bool TryPlaceAtScreenCenter()
    {
        var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        if (!TryGetPlacementPose(screenCenter, out var placementPose))
            return false;

        if (currentStep == PlacementStep.FirstNpc)
            PlaceFirstNpc(placementPose);
        else if (currentStep == PlacementStep.SecondNpc)
            PlaceSecondNpc(placementPose);
        else
            return false;

        return true;
    }

    void ResolveReferences()
    {
        if (manager == null)
            manager = FindAnyObjectByType<IndoorNpcTestManager>();

        if (raycastManager == null)
            raycastManager = FindAnyObjectByType<ARRaycastManager>();

        if (arCamera == null)
            arCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
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

    bool IsPointerOverUI(Vector2 tapPosition)
    {
        if (EventSystem.current == null)
            return false;

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
#endif

        return EventSystem.current.IsPointerOverGameObject();
    }

    bool TryGetPlacementPose(Vector2 screenPosition, out Pose placementPose)
    {
        if (raycastManager != null &&
            raycastManager.Raycast(screenPosition, Hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = Hits[0].pose;
            placementPose = new Pose(hitPose.position, GetUprightRotation());
            return true;
        }

        if (allowCameraForwardFallback && arCamera != null)
        {
            placementPose = new Pose(
                arCamera.transform.position +
                arCamera.transform.forward * editorFallbackDistanceMeters +
                Vector3.up * fallbackVerticalOffsetMeters,
                GetUprightRotation());
            return true;
        }

        placementPose = default;
        return false;
    }

    Quaternion GetUprightRotation()
    {
        ResolveReferences();

        if (arCamera == null)
            return Quaternion.identity;

        var forward = Vector3.ProjectOnPlane(arCamera.transform.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.forward;

        return Quaternion.LookRotation(forward.normalized, Vector3.up);
    }
}

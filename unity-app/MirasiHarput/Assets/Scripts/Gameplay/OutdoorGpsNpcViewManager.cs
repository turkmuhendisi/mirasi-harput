using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DefaultExecutionOrder(20)]
public class OutdoorGpsNpcViewManager : MonoBehaviour
{
    const double MetersPerDegreeLatitude = 111320d;

    [Header("Ortam")]
    [Tooltip("Açıkken yalnızca dış mekân GPS NPC ortamlarında (ParkOutdoorTest, MesireAlani) görseller oluşturulur.")]
    [SerializeField] bool restrictToOutdoorGpsEnvironments = true;

    [Header("Referanslar (boşsa sahne içinden aranır)")]
    [SerializeField] Transform xrOrigin;
    [SerializeField] JsonDataLoader dataLoader;
    [SerializeField] LocationTriggerManager locationTriggerManager;
    [SerializeField] QuestProgressManager questProgressManager;

    [Header("Indoor ile aynı NPC görselleri")]
    [SerializeField] GameObject npcGuide1Prefab = null;
    [SerializeField] GameObject npcGuide2Prefab = null;
    [SerializeField, Min(0.1f)] float fallbackNpcHeightMeters = 1.55f;

    [Header("Görünüm")]
    [SerializeField] float npcGroundClearanceY = 0.05f;
    [SerializeField] float maxVisibleDistanceMeters = 1000f;
    [SerializeField] float currentTargetScale = 1.12f;
    [SerializeField] Color colorDefault = Color.white;
    [SerializeField] Color colorCurrentTarget = Color.white;
    [SerializeField] Color colorCompleted = new Color(0.7f, 0.7f, 0.7f, 1f);

    [Header("Konumlandırma")]
    [Tooltip("Yer çakılı tutmak için NPC'ler bir kez yerleştirilir, sonra hareket etmez.")]
    [SerializeField] bool anchorOnceWhenGpsReady = true;
    [SerializeField] float eyeHeightOffsetMeters = 1.45f;

    [Header("Pusula")]
    [SerializeField] bool useCompassWhenAvailable = true;
    [SerializeField] float compassHeadingOffsetDegrees = 0f;
    [SerializeField, Min(0.5f)] float compassWarmupSeconds = 1.5f;

    [Header("Tıklama")]
    [SerializeField] bool enableTapInteraction = true;
    [SerializeField] LayerMask tapRaycastMask = ~0;
    [SerializeField, Min(16f)] float npcScreenTapRadiusPixels = 220f;
    [SerializeField, Min(0.5f)] float tapWorldDistanceMeters = 4f;

    Transform markersParent;
    readonly Dictionary<string, OutdoorGpsNpcMarker> markers = new Dictionary<string, OutdoorGpsNpcMarker>();
    bool markersAnchored;
    float compassWarmupRemaining;
    double lastAnchorLatitude;
    double lastAnchorLongitude;
    bool hasAnchorGpsSample;
    bool subscribedToGpsUpdates;
    bool subscribedToLocationTriggers;

    [Header("Yeniden hizalama")]
    [SerializeField] bool reanchorWhenPlayerGpsMoves = true;
    [SerializeField, Min(1f)] float reanchorGpsMoveThresholdMeters = 8f;
    [Tooltip("GPS ile bu mesafenin içindeyken NPC dünya konumuna yerleştirilir.")]
    [SerializeField, Min(5f)] float gpsPlacementMaxDistanceMeters = 80f;
    [Tooltip("Tetik alanındayken NPC kameranın önüne alınır (görünürlük garantisi).")]
    [SerializeField] bool snapToCameraWhenInsideTrigger = true;
    [SerializeField, Min(0.5f)] float snapDistanceInFrontOfCameraMeters = 2.2f;

    void OnEnable()
    {
        ResolveDataLoader();
        if (dataLoader != null)
        {
            dataLoader.OnDataLoaded -= HandleDataLoaded;
            dataLoader.OnDataLoaded += HandleDataLoaded;
        }

        ResolveLocationTrigger();
        ResolveQuestProgress();
        SubscribeToLocationTriggers();
        SubscribeToGpsUpdates();

        if (useCompassWhenAvailable)
        {
            Input.compass.enabled = true;
            compassWarmupRemaining = compassWarmupSeconds;
        }
    }

    void OnDisable()
    {
        if (dataLoader != null)
            dataLoader.OnDataLoaded -= HandleDataLoaded;

        UnsubscribeFromGpsUpdates();
        UnsubscribeFromLocationTriggers();
    }

    void SubscribeToLocationTriggers()
    {
        if (subscribedToLocationTriggers)
            return;

        ResolveLocationTrigger();
        if (locationTriggerManager == null)
            return;

        locationTriggerManager.OnLocationTriggered -= HandleLocationTriggered;
        locationTriggerManager.OnLocationTriggered += HandleLocationTriggered;
        subscribedToLocationTriggers = true;
    }

    void UnsubscribeFromLocationTriggers()
    {
        if (!subscribedToLocationTriggers || locationTriggerManager == null)
            return;

        locationTriggerManager.OnLocationTriggered -= HandleLocationTriggered;
        subscribedToLocationTriggers = false;
    }

    void HandleLocationTriggered(LocationData location, QuestData quest)
    {
        OpenQuestInteractionForLocation(location);
    }

    void SubscribeToGpsUpdates()
    {
        if (subscribedToGpsUpdates)
            return;

        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.OnLocationUpdated += HandleGpsUpdated;
            subscribedToGpsUpdates = true;
        }
    }

    void UnsubscribeFromGpsUpdates()
    {
        if (!subscribedToGpsUpdates || LocationManager.Instance == null)
            return;

        LocationManager.Instance.OnLocationUpdated -= HandleGpsUpdated;
        subscribedToGpsUpdates = false;
    }

    void HandleGpsUpdated(LocationManager _)
    {
        if (!reanchorWhenPlayerGpsMoves || !hasAnchorGpsSample)
            return;

        if (locationTriggerManager == null ||
            !locationTriggerManager.TryGetPlayerCoordinates(out var lat, out var lon))
            return;

        var moved = (float)LocationManager.CalculateDistanceMeters(lastAnchorLatitude, lastAnchorLongitude, lat, lon);
        if (moved >= reanchorGpsMoveThresholdMeters)
            markersAnchored = false;
    }

    void Start()
    {
        if (!ShouldRun())
        {
            Destroy(gameObject);
            return;
        }

        TryCreateMarkersParent();

        if (dataLoader != null && dataLoader.IsLoaded)
            HandleDataLoaded(dataLoader);
    }

    void Update()
    {
        if (!ShouldRun())
            return;

        SubscribeToGpsUpdates();
        TryCreateMarkersParent();

        if (compassWarmupRemaining > 0f)
            compassWarmupRemaining -= Time.deltaTime;

        if (markersParent != null && locationTriggerManager != null && dataLoader != null && dataLoader.IsLoaded)
        {
            if (markers.Count == 0)
                HandleDataLoaded(dataLoader);

            if (ShouldReanchorMarkers())
                TryAnchorMarkers();

            UpdateMarkerStates();
        }

        if (enableTapInteraction)
            HandleTapInput();
    }

    bool ShouldReanchorMarkers()
    {
        if (markers.Count == 0)
            return false;

        if (!markersAnchored)
            return true;

        if (!anchorOnceWhenGpsReady)
            return true;

        if (!reanchorWhenPlayerGpsMoves || locationTriggerManager == null)
            return false;

        if (!locationTriggerManager.TryGetPlayerCoordinates(out var lat, out var lon))
            return false;

        if (!hasAnchorGpsSample)
            return true;

        var moved = (float)LocationManager.CalculateDistanceMeters(lastAnchorLatitude, lastAnchorLongitude, lat, lon);
        return moved >= reanchorGpsMoveThresholdMeters;
    }

    void TryAnchorMarkers()
    {
        if (markers.Count == 0)
            return;

        if (locationTriggerManager == null ||
            !locationTriggerManager.TryGetPlayerCoordinates(out var userLat, out var userLon))
            return;

        if (compassWarmupRemaining > 0f && useCompassWhenAvailable && Input.compass.enabled)
        {
            var headingProbe = Input.compass.trueHeading;
            if (headingProbe < 0f)
                headingProbe = Input.compass.magneticHeading;
            if (headingProbe < 0f)
                compassWarmupRemaining = 0f;
        }

        if (compassWarmupRemaining > 0f)
            return;

        var heading = ResolveCompassHeadingDegrees();
        var cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        if (cam == null)
            return;

        var groundY = cam.transform.position.y - eyeHeightOffsetMeters;
        var originX = cam.transform.position.x;
        var originZ = cam.transform.position.z;

        foreach (var kv in markers)
        {
            var loc = dataLoader.GetLocationById(kv.Key);
            if (loc == null || kv.Value == null)
                continue;

            var gpsDistance = (float)LocationManager.CalculateDistanceMeters(
                userLat, userLon, loc.latitude, loc.longitude);

            if (gpsDistance > gpsPlacementMaxDistanceMeters)
            {
                kv.Value.SetVisible(false);
                continue;
            }

            GpsDeltaToEastNorthMeters(userLat, userLon, loc.latitude, loc.longitude, out var east, out var north);

            var rotation = Quaternion.Euler(0f, -heading + compassHeadingOffsetDegrees, 0f);
            var rotated = rotation * new Vector3((float)east, 0f, (float)north);

            var worldPos = new Vector3(
                originX + rotated.x,
                groundY + npcGroundClearanceY,
                originZ + rotated.z);

            kv.Value.transform.SetParent(markersParent, true);
            kv.Value.transform.position = worldPos;
            kv.Value.SetVisible(true);
            kv.Value.RefreshRenderers();
        }

        markersAnchored = true;
        lastAnchorLatitude = userLat;
        lastAnchorLongitude = userLon;
        hasAnchorGpsSample = true;
    }

    void UpdateMarkerStates()
    {
        ResolveQuestProgress();

        var cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        var currentTarget = locationTriggerManager.CurrentTargetLocation;
        var hasGps = locationTriggerManager.TryGetPlayerCoordinates(out var userLat, out var userLon);

        foreach (var loc in dataLoader.Locations)
        {
            if (loc == null || string.IsNullOrEmpty(loc.id))
                continue;

            if (!markers.TryGetValue(loc.id, out var marker) || marker == null)
                continue;

            var isCurrent = currentTarget != null && currentTarget.id == loc.id;
            var quest = dataLoader.GetQuestByLocationId(loc.id);
            var isDone = quest != null && questProgressManager != null &&
                questProgressManager.IsQuestCompleted(quest.id);

            var gpsDistance = hasGps
                ? (float)LocationManager.CalculateDistanceMeters(userLat, userLon, loc.latitude, loc.longitude)
                : float.MaxValue;

            var visible = hasGps && gpsDistance <= gpsPlacementMaxDistanceMeters;
            if (cam != null && maxVisibleDistanceMeters > 0f && visible)
            {
                var worldDist = Vector3.Distance(cam.transform.position, marker.transform.position);
                visible = worldDist <= maxVisibleDistanceMeters;
            }

            if (ShouldSnapMarkerToCamera(isCurrent, gpsDistance, loc))
                SnapMarkerInFrontOfCamera(marker, cam);

            marker.SetVisible(visible);
            marker.gameObject.SetActive(visible);
            marker.ApplyVisualState(isCurrent, isDone, colorCurrentTarget, colorDefault, colorCompleted, currentTargetScale);

            var arNpc = marker.GetComponent<ARNpcController>();
            if (arNpc != null)
                arNpc.SetAsTarget(isCurrent);
        }
    }

    bool ShouldSnapMarkerToCamera(bool isCurrent, float gpsDistance, LocationData loc)
    {
        if (!snapToCameraWhenInsideTrigger || !isCurrent || loc == null)
            return false;

        var triggerRadius = loc.triggerRadiusMeters > 0f ? loc.triggerRadiusMeters : 2f;
        return gpsDistance <= triggerRadius + 0.5f;
    }

    void SnapMarkerInFrontOfCamera(OutdoorGpsNpcMarker marker, Camera cam)
    {
        if (marker == null || cam == null)
            return;

        var forward = cam.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            forward = cam.transform.rotation * Vector3.forward;

        forward.Normalize();
        var groundY = cam.transform.position.y - eyeHeightOffsetMeters;
        var worldPos = cam.transform.position + forward * snapDistanceInFrontOfCameraMeters;
        worldPos.y = groundY + npcGroundClearanceY;

        marker.transform.position = worldPos;
        marker.RefreshRenderers();
    }

    void HandleTapInput()
    {
        if (!TryGetTapPosition(out var tapPosition))
            return;

        TryHandleTapAt(tapPosition);
    }

    void TryHandleTapAt(Vector2 screenPosition)
    {
        var cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        if (cam == null)
            return;

        var ray = cam.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out var hit, 200f, tapRaycastMask, QueryTriggerInteraction.Collide))
        {
            var marker = hit.collider.GetComponentInParent<OutdoorGpsNpcMarker>();
            if (marker != null)
            {
                TriggerLocationForMarker(marker.LocationId);
                return;
            }
        }

        TryTapNearestMarker(screenPosition, cam);
    }

    void TryTapNearestMarker(Vector2 screenPosition, Camera cam)
    {
        OutdoorGpsNpcMarker closest = null;
        var closestDistance = float.MaxValue;

        foreach (var kv in markers)
        {
            var marker = kv.Value;
            if (marker == null || !marker.gameObject.activeInHierarchy)
                continue;

            var screenPoint = cam.WorldToScreenPoint(marker.transform.position + Vector3.up * 0.9f);
            if (screenPoint.z < 0f)
                continue;

            var distance = Vector2.Distance(screenPosition, new Vector2(screenPoint.x, screenPoint.y));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = marker;
            }
        }

        if (closest == null || closestDistance > npcScreenTapRadiusPixels)
            return;

        TriggerLocationForMarker(closest.LocationId);
    }

    void TriggerLocationForMarker(string locationId)
    {
        if (string.IsNullOrEmpty(locationId) || dataLoader == null || locationTriggerManager == null)
            return;

        var loc = dataLoader.GetLocationById(locationId);
        if (loc == null)
            return;

        if (!CanInteractWithLocation(loc, out var marker))
            return;

        var currentTarget = locationTriggerManager.CurrentTargetLocation;
        if (locationTriggerManager.TriggerOnlyCurrentRouteLocation &&
            currentTarget != null &&
            currentTarget.id != loc.id)
            return;

        if (locationTriggerManager.IsLocationTriggered(loc.id))
        {
            OpenQuestInteractionForLocation(loc);
            return;
        }

        locationTriggerManager.TriggerLocation(loc);
        OpenQuestInteractionForLocation(loc);
    }

    bool CanInteractWithLocation(LocationData loc, out OutdoorGpsNpcMarker marker)
    {
        marker = markers.TryGetValue(loc.id, out var found) ? found : null;

        if (locationTriggerManager.IsInsideTriggerRadius &&
            locationTriggerManager.CurrentTargetLocation != null &&
            locationTriggerManager.CurrentTargetLocation.id == loc.id)
            return true;

        if (marker != null)
        {
            var cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            if (cam != null)
            {
                var worldDist = Vector3.Distance(cam.transform.position, marker.transform.position);
                if (worldDist <= tapWorldDistanceMeters)
                    return true;
            }
        }

        if (!locationTriggerManager.TryGetPlayerCoordinates(out var userLat, out var userLon))
            return false;

        var gpsDistance = (float)LocationManager.CalculateDistanceMeters(
            userLat, userLon, loc.latitude, loc.longitude);
        var interactionRadius = loc.triggerRadiusMeters > 0f ? loc.triggerRadiusMeters : 2f;
        return gpsDistance <= interactionRadius + 1f;
    }

    void OpenQuestInteractionForLocation(LocationData loc)
    {
        if (loc == null)
            return;

        ResolveQuestProgress();

        var quest = dataLoader != null ? dataLoader.GetQuestByLocationId(loc.id) : null;
        if (quest == null || questProgressManager == null)
            return;

        var questPanel = GameObject.Find("QuestInteractionPanel");
        if (questPanel != null && !questPanel.activeSelf)
            questPanel.SetActive(true);

        questProgressManager.HandleLocationTriggered(loc, quest);
        questProgressManager.SetQuestAvailable(quest.id);
        questProgressManager.StartQuest(quest.id);
    }

    static bool TryGetTapPosition(out Vector2 tapPosition)
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
            if (touch.phase == TouchPhase.Began)
            {
                tapPosition = touch.position;
                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            tapPosition = Input.mousePosition;
            return true;
        }
#endif

        tapPosition = Vector2.zero;
        return false;
    }

    bool ShouldRun()
    {
        if (!restrictToOutdoorGpsEnvironments)
            return true;

        var configs = FindObjectsByType<DataEnvironmentConfig>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (var i = 0; i < configs.Length; i++)
        {
            if (configs[i] != null && configs[i].ShowsOutdoorGpsNpcs())
                return true;
        }

        return false;
    }

    void TryCreateMarkersParent()
    {
        if (markersParent != null)
            return;

        var existing = GameObject.Find("GpsRouteNpcMarkers");
        if (existing != null)
        {
            markersParent = existing.transform;
            return;
        }

        var root = new GameObject("GpsRouteNpcMarkers");
        root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        markersParent = root.transform;
    }

    void ResolveXrOrigin()
    {
        if (xrOrigin != null)
            return;

        var cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        if (cam != null)
            xrOrigin = cam.transform.root;
    }

    void ResolveDataLoader()
    {
        if (dataLoader != null)
            return;

        if (JsonDataLoader.Instance != null)
        {
            dataLoader = JsonDataLoader.Instance;
            return;
        }

        var loaders = FindObjectsByType<JsonDataLoader>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        dataLoader = loaders.Length > 0 ? loaders[0] : null;
    }

    void ResolveLocationTrigger()
    {
        if (locationTriggerManager != null)
            return;

        var triggers = FindObjectsByType<LocationTriggerManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        locationTriggerManager = triggers.Length > 0 ? triggers[0] : null;
    }

    void ResolveQuestProgress()
    {
        if (questProgressManager != null)
            return;

        questProgressManager = FindAnyObjectByType<QuestProgressManager>(FindObjectsInactive.Include);
    }

    void HandleDataLoaded(JsonDataLoader loader)
    {
        dataLoader = loader;
        if (!ShouldRun())
            return;

        TryCreateMarkersParent();
        if (markersParent == null)
            return;

        ClearMarkers();
        markersAnchored = false;
        hasAnchorGpsSample = false;
        compassWarmupRemaining = compassWarmupSeconds;

        if (loader.Locations == null)
            return;

        for (var i = 0; i < loader.Locations.Count; i++)
        {
            var loc = loader.Locations[i];
            if (loc == null || string.IsNullOrEmpty(loc.id))
                continue;

            var prefab = ResolveNpcPrefab(loc);
            var resourcePath = ResolveNpcResourcePath(loc);
            var displayTitle = ResolveNpcDisplayTitle(loc);
            var go = prefab != null
                ? NpcVisualSpawn.Create(loc.name, prefab, resourcePath, fallbackNpcHeightMeters)
                : NpcVisualSpawn.CreateOutdoorAr(loc.name, resourcePath, fallbackNpcHeightMeters, displayTitle);

            go.name = "Npc_" + loc.id;
            go.transform.SetParent(markersParent, false);

            var capsuleCol = go.GetComponent<CapsuleCollider>();
            if (capsuleCol != null)
                Destroy(capsuleCol);

            var controller = go.GetComponent<ARNpcController>();
            if (controller == null)
                controller = go.AddComponent<ARNpcController>();
            controller.SetManager(null);
            controller.Setup(loc.npcId, loc.name, false);
            controller.SetFaceCamera(false);

            var billboard = go.GetComponent<NpcArBillboardVisual>();
            if (billboard != null)
                billboard.SetFaceCameraEnabled(true);

            var marker = go.AddComponent<OutdoorGpsNpcMarker>();
            marker.Initialize(loc.id);
            markers[loc.id] = marker;

            EnsureTapCollider(go);

            go.SetActive(true);
        }
    }

    GameObject ResolveNpcPrefab(LocationData loc)
    {
        if (loc != null && !string.IsNullOrEmpty(loc.imageKey) &&
            loc.imageKey.StartsWith("npc_guide", StringComparison.OrdinalIgnoreCase))
            return null;

        var npcId = loc != null ? loc.npcId : null;
        return UsesSecondGuideCharacter(npcId) ? npcGuide2Prefab : npcGuide1Prefab;
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
            npcId.IndexOf("arap_baba", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static void EnsureTapCollider(GameObject go)
    {
        var box = go.GetComponent<BoxCollider>();
        if (box == null)
            box = go.AddComponent<BoxCollider>();

        box.isTrigger = false;
        box.size = new Vector3(1.4f, 2f, 0.35f);
        box.center = new Vector3(0f, 1f, 0f);
    }

    void ClearMarkers()
    {
        foreach (var kv in markers)
        {
            if (kv.Value != null && kv.Value.gameObject != null)
                Destroy(kv.Value.gameObject);
        }

        markers.Clear();
    }

    float ResolveCompassHeadingDegrees()
    {
        if (!useCompassWhenAvailable || !Input.compass.enabled)
            return GetCameraYawDegrees();

        var raw = Input.compass.trueHeading;
        if (raw < 0f)
            raw = Input.compass.magneticHeading;

        if (raw < 0f)
            return GetCameraYawDegrees();

        return raw;
    }

    static float GetCameraYawDegrees()
    {
        var cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        return cam != null ? cam.transform.eulerAngles.y : 0f;
    }

    static void GpsDeltaToEastNorthMeters(double userLat, double userLon, double targetLat, double targetLon, out double east, out double north)
    {
        var latRad = userLat * Math.PI / 180d;
        var metersPerDegreeLon = MetersPerDegreeLatitude * Math.Cos(latRad);
        north = (targetLat - userLat) * MetersPerDegreeLatitude;
        east = (targetLon - userLon) * metersPerDegreeLon;
    }
}

public class OutdoorGpsNpcMarker : MonoBehaviour
{
    string locationId;
    Renderer[] allRenderers;
    Material[] materialInstances;

    public string LocationId => locationId;

    public void Initialize(string id)
    {
        locationId = id;
        RefreshRenderers();
    }

    public void RefreshRenderers()
    {
        var found = GetComponentsInChildren<Renderer>(true);
        var npcRenderers = new List<Renderer>();
        var mats = new List<Material>();

        for (var i = 0; i < found.Length; i++)
        {
            var r = found[i];
            if (r == null || IsTitleRenderer(r.transform))
                continue;

            npcRenderers.Add(r);
            if (r.material != null)
                mats.Add(r.material);
        }

        allRenderers = npcRenderers.ToArray();
        materialInstances = mats.ToArray();

        for (var i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] != null)
                allRenderers[i].enabled = true;
        }
    }

    static bool IsTitleRenderer(Transform transform)
    {
        var current = transform;
        while (current != null)
        {
            if (current.name == "NpcTitle")
                return true;

            current = current.parent;
        }

        return false;
    }

    public void SetVisible(bool visible)
    {
        if (allRenderers == null)
            return;

        for (var i = 0; i < allRenderers.Length; i++)
        {
            var r = allRenderers[i];
            if (r != null)
                r.enabled = visible;
        }
    }

    public void ApplyVisualState(bool isCurrent, bool isCompleted, Color current, Color normal, Color done, float currentScaleMul)
    {
        var c = isCompleted ? done : isCurrent ? current : normal;
        c.a = 1f;

        for (var i = 0; i < allRenderers.Length; i++)
        {
            var r = allRenderers[i];
            if (r == null)
                continue;

            var sprite = r as SpriteRenderer;
            if (sprite != null)
            {
                sprite.color = new Color(1f, 1f, 1f, 1f);
                continue;
            }
        }

        if (GetComponent<NpcArBillboardVisual>() != null)
            return;

        if (materialInstances != null)
        {
            for (var i = 0; i < materialInstances.Length; i++)
            {
                var m = materialInstances[i];
                if (m == null)
                    continue;

                if (m.HasProperty("_BaseColor"))
                    m.SetColor("_BaseColor", c);
                if (m.HasProperty("_Color"))
                    m.SetColor("_Color", c);
            }
        }
    }

    void OnDestroy()
    {
        if (materialInstances == null)
            return;

        for (var i = 0; i < materialInstances.Length; i++)
        {
            if (materialInstances[i] != null)
                Destroy(materialInstances[i]);
        }
    }
}

static class OutdoorGpsNpcViewRuntimeBootstrap
{
    public static void EnsureHost()
    {
        if (UnityEngine.Object.FindAnyObjectByType<OutdoorGpsNpcViewManager>() != null)
            return;

        var go = new GameObject(nameof(OutdoorGpsNpcViewManager));
        go.AddComponent<OutdoorGpsNpcViewManager>();
    }
}

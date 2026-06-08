#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Varsayılan QR akış prefab'ını oluşturur. Sonrasında görseli Unity Editor'da özelleştirin.
/// </summary>
public static class QrFlowUIPrefabCreator
{
    const string PrefabFolder = "Assets/Resources/UI/Qr";
    const string PrefabPath = PrefabFolder + "/QrFlowUI.prefab";
    const string RouteItemPath = PrefabFolder + "/QrRouteListItem.prefab";

    [MenuItem("Mirasi Harput/UI/Create QR Flow UI Prefabs (Legacy)")]
    public static void CreatePrefabs()
    {
        EnsureFolder(PrefabFolder);

        var routeItemPrefab = CreateRouteListItemPrefab();
        var flowPrefab = CreateFlowPrefab(routeItemPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = flowPrefab;
        EditorGUIUtility.PingObject(flowPrefab);

        Debug.Log("[QrFlowUI] Prefab'lar oluşturuldu: " + PrefabPath + "\nHierarchy'yi düzenleyip kaydedin. Runtime: Resources/" + QrFlowUIView.PrefabResourcesPath);
    }

    static GameObject CreateRouteListItemPrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(RouteItemPath);
        if (existing != null)
            return existing;

        var root = new GameObject("QrRouteListItem", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(QrRouteListItem), typeof(LayoutElement));
        var image = root.GetComponent<Image>();
        image.color = new Color(0.79f, 0.6f, 0.28f, 1f);
        root.GetComponent<LayoutElement>().minHeight = 72f;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(root.transform, false);
        var labelRect = labelGo.GetComponent<RectTransform>();
        Stretch(labelRect);
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "Rota adı";
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        AssignDefaultFont(tmp);

        var item = root.GetComponent<QrRouteListItem>();
        var serialized = new SerializedObject(item);
        serialized.FindProperty("button").objectReferenceValue = root.GetComponent<Button>();
        serialized.FindProperty("labelText").objectReferenceValue = tmp;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        return SavePrefab(root, RouteItemPath);
    }

    static GameObject CreateFlowPrefab(GameObject routeItemPrefab)
    {
        var root = new GameObject(
            "QrFlowUI",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(QrFlowUIView),
            typeof(QrFlowController));

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 2000;

        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        var flowRoot = CreateStretchChild(root.transform, "Screens");
        Stretch(flowRoot.GetComponent<RectTransform>());

        var welcome = CreateScreen(flowRoot.transform, "WelcomeScreen", "Mirası Harput", true);
        CreateButton(welcome.transform, "ContinueButton", "Başla", new Vector2(0.2f, 0.12f), new Vector2(0.8f, 0.22f));

        var routeSelect = CreateScreen(flowRoot.transform, "RouteSelectScreen", "Rota Seç", true);
        CreateText(routeSelect.transform, "Hint", "Bir rota seçin:", 22, new Vector2(0.08f, 0.74f), new Vector2(0.92f, 0.8f), TextAlignmentOptions.TopLeft, ColorStyle.MutedText);
        var content = CreateRouteScroll(routeSelect.transform);
        CreateButton(routeSelect.transform, "BackButton", "Geri", new Vector2(0.08f, 0.06f), new Vector2(0.35f, 0.14f));

        var confirm = CreateModal(flowRoot.transform, "StartConfirmModal");
        var card = CreatePanel(confirm.transform, "Card", new Vector2(0.1f, 0.28f), new Vector2(0.9f, 0.72f), DefaultPanelColor);
        CreateText(card.transform, "RouteName", "Rota", 30, new Vector2(0.06f, 0.72f), new Vector2(0.94f, 0.95f), TextAlignmentOptions.Top, ColorStyle.PrimaryText);
        CreateText(card.transform, "Body", "Bu rotayı başlatmak istiyor musunuz?", 22, new Vector2(0.06f, 0.38f), new Vector2(0.94f, 0.7f), TextAlignmentOptions.Top, ColorStyle.MutedText);
        CreateButton(card.transform, "Cancel", "Vazgeç", new Vector2(0.06f, 0.08f), new Vector2(0.46f, 0.2f));
        CreateButton(card.transform, "Start", "Rotayı Başlat", new Vector2(0.54f, 0.08f), new Vector2(0.94f, 0.2f));

        var hub = CreateScreen(flowRoot.transform, "QrHubScreen", null, true);
        CreateText(hub.transform, "RouteName", "Rota", 32, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.94f), TextAlignmentOptions.Top, ColorStyle.PrimaryText);
        CreateText(hub.transform, "Checkpoint", "Durak 1 / 5", 24, new Vector2(0.08f, 0.76f), new Vector2(0.92f, 0.82f), TextAlignmentOptions.TopLeft, ColorStyle.AccentText);
        CreateText(hub.transform, "Target", "Hedef: —", 26, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.74f), TextAlignmentOptions.TopLeft, ColorStyle.PrimaryText);
        CreateText(hub.transform, "Next", "Sonraki: —", 22, new Vector2(0.08f, 0.58f), new Vector2(0.92f, 0.64f), TextAlignmentOptions.TopLeft, ColorStyle.MutedText);
        CreateText(hub.transform, "Status", "QR kodu taramak için butona basın.", 20, new Vector2(0.08f, 0.44f), new Vector2(0.92f, 0.56f), TextAlignmentOptions.TopLeft, ColorStyle.MutedText);
        CreateButton(hub.transform, "ScanButton", "QR Kodu Tarat", new Vector2(0.12f, 0.22f), new Vector2(0.88f, 0.34f));
        CreateButton(hub.transform, "ChangeRoute", "Rotayı Değiştir", new Vector2(0.12f, 0.1f), new Vector2(0.88f, 0.18f));

        var scan = CreateScreen(flowRoot.transform, "QrScanScreen", null, false);
        scan.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.12f);
        var topBar = CreateBar(scan.transform, "TopBar", new Vector2(0f, 0.84f), new Vector2(1f, 1f));
        CreateButton(topBar.transform, "Close", "Kapat", new Vector2(0.04f, 0.12f), new Vector2(0.28f, 0.88f));
        CreateText(topBar.transform, "Hint", "QR kodu çerçeveye hizalayın", 24, new Vector2(0.3f, 0.1f), new Vector2(0.96f, 0.9f), TextAlignmentOptions.Center, ColorStyle.PrimaryText);
        var bottomBar = CreateBar(scan.transform, "BottomBar", new Vector2(0f, 0f), new Vector2(1f, 0.2f));
        CreateText(bottomBar.transform, "Status", "Kamera açılıyor…", 20, new Vector2(0.06f, 0.42f), new Vector2(0.94f, 0.92f), TextAlignmentOptions.Center, ColorStyle.MutedText);
        CreateButton(bottomBar.transform, "BackToHub", "QR Ekranına Dön", new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.36f));

        var ar = CreateScreen(flowRoot.transform, "ArExperienceScreen", null, false);
        ar.GetComponent<Image>().raycastTarget = false;
        ar.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        var arTop = CreateBar(ar.transform, "TopBar", new Vector2(0f, 0.86f), new Vector2(1f, 1f));
        CreateText(arTop.transform, "Route", "Rota bilgisi", 20, new Vector2(0.04f, 0.15f), new Vector2(0.96f, 0.95f), TextAlignmentOptions.TopLeft, ColorStyle.PrimaryText);
        var arBottom = CreateBar(ar.transform, "BottomBar", new Vector2(0f, 0f), new Vector2(1f, 0.14f));
        CreateText(arBottom.transform, "Status", "", 20, new Vector2(0.06f, 0.35f), new Vector2(0.94f, 0.95f), TextAlignmentOptions.Center, ColorStyle.MutedText);
        CreateButton(arBottom.transform, "QuestButton", "Göreve Devam", new Vector2(0.12f, 0.18f), new Vector2(0.88f, 0.82f));
        CreateButton(arBottom.transform, "HubButton", "QR Ekranına Dön", new Vector2(0.12f, 0.05f), new Vector2(0.88f, 0.16f));

        var view = root.GetComponent<QrFlowUIView>();
        view.AutoWireByName();

        var viewSo = new SerializedObject(view);
        viewSo.FindProperty("routeListItemPrefab").objectReferenceValue = routeItemPrefab.GetComponent<QrRouteListItem>();
        viewSo.ApplyModifiedPropertiesWithoutUndo();

        return SavePrefab(root, PrefabPath);
    }

    static Transform CreateRouteScroll(Transform parent)
    {
        var scrollGo = new GameObject("RouteScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollGo.transform.SetParent(parent, false);
        SetAnchors(scrollGo.GetComponent<RectTransform>(), new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.72f));
        scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
        viewport.transform.SetParent(scrollGo.transform, false);
        Stretch(viewport.GetComponent<RectTransform>());
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        var layout = content.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = contentRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        return content.transform;
    }

    static GameObject CreateModal(Transform parent, string name)
    {
        var go = CreateScreen(parent, name, null, true);
        go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
        go.SetActive(false);
        return go;
    }

    static GameObject CreateScreen(Transform parent, string name, string title, bool opaque)
    {
        var panel = CreatePanel(parent, name, Vector2.zero, Vector2.one, opaque ? DefaultPanelColor : new Color(0f, 0f, 0f, 0.12f));
        if (!string.IsNullOrEmpty(title))
            CreateText(panel.transform, "Title", title, 36, new Vector2(0.08f, 0.55f), new Vector2(0.92f, 0.82f), TextAlignmentOptions.Center, ColorStyle.PrimaryText);
        panel.SetActive(name == "WelcomeScreen");
        return panel;
    }

    static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        SetAnchors(go.GetComponent<RectTransform>(), anchorMin, anchorMax);
        go.GetComponent<Image>().color = color;
        return go;
    }

    static GameObject CreateBar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        var bar = CreatePanel(parent, name, anchorMin, anchorMax, new Color(0f, 0f, 0f, 0.72f));
        return bar;
    }

    static GameObject CreateStretchChild(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static TMP_Text CreateText(Transform parent, string name, string text, int size, Vector2 min, Vector2 max, TextAlignmentOptions align, ColorStyle style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        SetAnchors(go.GetComponent<RectTransform>(), min, max);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = ColorFor(style);
        AssignDefaultFont(tmp);
        return tmp;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        SetAnchors(go.GetComponent<RectTransform>(), anchorMin, anchorMax);
        go.GetComponent<Image>().color = DefaultAccentColor;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        Stretch(labelGo.GetComponent<RectTransform>());
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        AssignDefaultFont(tmp);
        return go.GetComponent<Button>();
    }

    static GameObject SavePrefab(GameObject root, string path)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/UI"))
            AssetDatabase.CreateFolder("Assets/Resources", "UI");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/UI/Qr"))
            AssetDatabase.CreateFolder("Assets/Resources/UI", "Qr");
    }

    static void AssignDefaultFont(TMP_Text tmp)
    {
        var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font != null)
            tmp.font = font;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    enum ColorStyle
    {
        PrimaryText,
        MutedText,
        AccentText
    }

    static readonly Color DefaultPanelColor = new Color(0.08f, 0.1f, 0.14f, 0.94f);
    static readonly Color DefaultAccentColor = new Color(0.79f, 0.6f, 0.28f, 1f);

    static Color ColorFor(ColorStyle style)
    {
        switch (style)
        {
            case ColorStyle.MutedText:
                return new Color(0.75f, 0.78f, 0.84f, 1f);
            case ColorStyle.AccentText:
                return DefaultAccentColor;
            default:
                return new Color(0.95f, 0.95f, 0.97f, 1f);
        }
    }
}
#endif

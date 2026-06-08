using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class LocationExperienceUICanvasFactory
{
    public const float RefWidth = 1080f;
    public const float RefHeight = 1920f;

    static readonly Color BackgroundBlack = Color.black;
    static readonly Color TextPrimary = new Color(0.93f, 0.93f, 0.93f, 1f);

    public static GameObject BuildRoot()
    {
        var root = new GameObject(
            "LocationExperienceUI",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(LocationAppUIView),
            typeof(AppFlowController));

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 2000;

        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(RefWidth, RefHeight);
        scaler.matchWidthOrHeight = 0.5f;

        var screens = CreateStretchChild(root.transform, "Screens");
        CreateQrReaderScreen(screens.transform);
        CreateLocationExperienceScreen(screens.transform);
        CreateToast(root.transform);

        var view = root.GetComponent<LocationAppUIView>();
        view.AutoWireByName();
        return root;
    }

    static void CreateQrReaderScreen(Transform parent)
    {
        var screen = CreatePanel(parent, "QrReaderScreen", Vector2.zero, Vector2.one, BackgroundBlack);
        screen.SetActive(true);

        var frame = new GameObject("ScanFrame", typeof(RectTransform));
        frame.transform.SetParent(screen.transform, false);
        var frameRect = frame.GetComponent<RectTransform>();
        frameRect.anchorMin = new Vector2(0.5f, 0.5f);
        frameRect.anchorMax = new Vector2(0.5f, 0.5f);
        frameRect.pivot = new Vector2(0.5f, 0.5f);
        frameRect.anchoredPosition = new Vector2(0f, 120f);
        frameRect.sizeDelta = new Vector2(520f, 520f);

        var previewGo = new GameObject("CameraPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        previewGo.transform.SetParent(frame.transform, false);
        Stretch(previewGo.GetComponent<RectTransform>());
        var preview = previewGo.GetComponent<RawImage>();
        preview.color = Color.white;
        preview.raycastTarget = false;

        var cornersRoot = new GameObject("Corners", typeof(RectTransform));
        cornersRoot.transform.SetParent(frame.transform, false);
        Stretch(cornersRoot.GetComponent<RectTransform>());
        CreateScanFrameCorners(cornersRoot.GetComponent<RectTransform>());

        var hint = CreateText(frame.transform, "Hint", "Scan QR", 28, Vector2.zero, Vector2.zero, TextAlignmentOptions.Center, TextPrimary);
        var hintRect = hint.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0f);
        hintRect.anchorMax = new Vector2(0.5f, 0f);
        hintRect.pivot = new Vector2(0.5f, 1f);
        hintRect.anchoredPosition = new Vector2(0f, -36f);
        hintRect.sizeDelta = new Vector2(400f, 48f);

        CreateText(screen.transform, "StatusText", string.Empty, 20, new Vector2(0.08f, 0.2f), new Vector2(0.92f, 0.26f), TextAlignmentOptions.Center, TextPrimary);
        CreateScanButton(screen.transform);
    }

    static void CreateScanButton(Transform parent)
    {
        var buttonGo = new GameObject("ScanButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(parent, false);
        var rect = buttonGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 72f);
        rect.sizeDelta = new Vector2(880f, 112f);

        var image = buttonGo.GetComponent<Image>();
        LocationExperienceUISprites.ApplyRoundedRect(image, Color.white, 256, 128, 64);

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(buttonGo.transform, false);
        Stretch(labelGo.GetComponent<RectTransform>());
        var label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = "SCAN";
        label.fontSize = 40;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.black;
    }

    static void CreateLocationExperienceScreen(Transform parent)
    {
        var screen = CreatePanel(parent, "LocationExperienceScreen", Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0f));
        screen.GetComponent<Image>().raycastTarget = false;
        screen.SetActive(false);

        var cameraArea = CreatePanel(screen.transform, "CameraArea", new Vector2(0f, 0.42f), new Vector2(1f, 1f), new Color(0f, 0f, 0f, 0f));
        cameraArea.GetComponent<Image>().raycastTarget = false;

        CreateBackButton(screen.transform);
        CreateInfoPanel(screen.transform);
    }

    static void CreateBackButton(Transform parent)
    {
        var buttonGo = new GameObject("BackButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(parent, false);
        var rect = buttonGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(40f, -48f);
        rect.sizeDelta = new Vector2(88f, 88f);

        var image = buttonGo.GetComponent<Image>();
        LocationExperienceUISprites.ApplyRoundedRect(image, BackgroundBlack, 128, 128, 24);

        var label = CreateText(buttonGo.transform, "Label", "←", 42, Vector2.zero, Vector2.one, TextAlignmentOptions.Center, Color.white);
        label.fontStyle = FontStyles.Bold;
    }

    static void CreateInfoPanel(Transform parent)
    {
        var panelGo = new GameObject("InfoPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelGo.transform.SetParent(parent, false);
        SetAnchors(panelGo.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0.42f));
        LocationExperienceUISprites.ApplyRoundedTopPanel(panelGo.GetComponent<Image>(), BackgroundBlack, 256, 256, 48);

        var audioButtonGo = new GameObject("AudioButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(AudioButtonVisual));
        audioButtonGo.transform.SetParent(panelGo.transform, false);
        var audioRect = audioButtonGo.GetComponent<RectTransform>();
        audioRect.anchorMin = new Vector2(1f, 1f);
        audioRect.anchorMax = new Vector2(1f, 1f);
        audioRect.pivot = new Vector2(1f, 1f);
        audioRect.anchoredPosition = new Vector2(-32f, -32f);
        audioRect.sizeDelta = new Vector2(80f, 80f);
        LocationExperienceUISprites.ApplyRoundedRect(audioButtonGo.GetComponent<Image>(), Color.white, 128, 128, 20);

        var barsRoot = CreateAudioBarsIcon(audioButtonGo.transform);
        var muteRoot = CreateAudioMuteIcon(audioButtonGo.transform);
        muteRoot.SetActive(false);
        audioButtonGo.GetComponent<AudioButtonVisual>().Configure(barsRoot, muteRoot);

        var scrollGo = new GameObject("DescriptionScroll", typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(panelGo.transform, false);
        SetAnchors(scrollGo.GetComponent<RectTransform>(), new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.82f));

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
        viewport.transform.SetParent(scrollGo.transform, false);
        Stretch(viewport.GetComponent<RectTransform>());
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
        viewport.GetComponent<Image>().raycastTarget = false;

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        var layout = content.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 8, 24);
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var description = CreateText(content.transform, "Description", string.Empty, 30, Vector2.zero, Vector2.one, TextAlignmentOptions.TopLeft, TextPrimary);
        description.fontStyle = FontStyles.Bold;
        description.enableWordWrapping = true;
        description.lineSpacing = 4f;
        description.overflowMode = TextOverflowModes.Overflow;
        var descriptionRect = description.GetComponent<RectTransform>();
        descriptionRect.anchorMin = new Vector2(0f, 1f);
        descriptionRect.anchorMax = new Vector2(1f, 1f);
        descriptionRect.pivot = new Vector2(0.5f, 1f);
        descriptionRect.sizeDelta = Vector2.zero;
        description.gameObject.AddComponent<LayoutElement>().minHeight = 120f;

        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = contentRect;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
    }

    static GameObject CreateAudioBarsIcon(Transform parent)
    {
        var root = new GameObject("BarsIcon", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        Stretch(root.GetComponent<RectTransform>());

        CreateAudioBar(root.transform, "Bar1", new Vector2(-18f, 0f), 18f, 36f);
        CreateAudioBar(root.transform, "Bar2", new Vector2(0f, 0f), 18f, 52f);
        CreateAudioBar(root.transform, "Bar3", new Vector2(18f, 0f), 18f, 40f);
        return root;
    }

    static void CreateAudioBar(Transform parent, string name, Vector2 position, float width, float height)
    {
        var bar = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bar.transform.SetParent(parent, false);
        var rect = bar.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(width, height);
        LocationExperienceUISprites.ApplyRoundedRect(bar.GetComponent<Image>(), Color.black, 32, 64, 8);
    }

    static GameObject CreateAudioMuteIcon(Transform parent)
    {
        var root = new GameObject("MuteIcon", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        Stretch(root.GetComponent<RectTransform>());

        var speaker = new GameObject("Speaker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        speaker.transform.SetParent(root.transform, false);
        var speakerRect = speaker.GetComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0.5f, 0.5f);
        speakerRect.anchorMax = new Vector2(0.5f, 0.5f);
        speakerRect.pivot = new Vector2(0.5f, 0.5f);
        speakerRect.anchoredPosition = new Vector2(-6f, 0f);
        speakerRect.sizeDelta = new Vector2(28f, 28f);
        speaker.GetComponent<Image>().color = Color.black;

        var slash = new GameObject("Slash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        slash.transform.SetParent(root.transform, false);
        var slashRect = slash.GetComponent<RectTransform>();
        slashRect.anchorMin = new Vector2(0.5f, 0.5f);
        slashRect.anchorMax = new Vector2(0.5f, 0.5f);
        slashRect.pivot = new Vector2(0.5f, 0.5f);
        slashRect.anchoredPosition = Vector2.zero;
        slashRect.sizeDelta = new Vector2(52f, 6f);
        slashRect.localEulerAngles = new Vector3(0f, 0f, -42f);
        slash.GetComponent<Image>().color = Color.black;

        return root;
    }

    static void CreateToast(Transform parent)
    {
        var toastRoot = CreatePanel(parent, "Toast", new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.86f), new Color(0.12f, 0.12f, 0.12f, 0.92f));
        var canvasGroup = toastRoot.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        var toast = toastRoot.AddComponent<LocationToast>();
        var text = CreateText(toastRoot.transform, "Message", string.Empty, 22, new Vector2(0.04f, 0.1f), new Vector2(0.96f, 0.9f), TextAlignmentOptions.Center, TextPrimary);
        toast.Configure(canvasGroup, text);
        toastRoot.SetActive(false);
    }

    static void CreateScanFrameCorners(RectTransform frame)
    {
        const float armLength = 56f;
        const float thickness = 5f;

        CreateBracketArm(frame, "TL_H", new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(armLength, thickness));
        CreateBracketArm(frame, "TL_V", new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(thickness, armLength));

        CreateBracketArm(frame, "TR_H", new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(armLength, thickness));
        CreateBracketArm(frame, "TR_V", new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(thickness, armLength));

        CreateBracketArm(frame, "BL_H", new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, new Vector2(armLength, thickness));
        CreateBracketArm(frame, "BL_V", new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, new Vector2(thickness, armLength));

        CreateBracketArm(frame, "BR_H", new Vector2(1f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(armLength, thickness));
        CreateBracketArm(frame, "BR_V", new Vector2(1f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(thickness, armLength));
    }

    static void CreateBracketArm(RectTransform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        go.GetComponent<Image>().color = Color.white;
        go.GetComponent<Image>().raycastTarget = false;
    }

    static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        SetAnchors(go.GetComponent<RectTransform>(), anchorMin, anchorMax);
        go.GetComponent<Image>().color = color;
        return go;
    }

    static GameObject CreateStretchChild(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Stretch(go.GetComponent<RectTransform>());
        return go;
    }

    static TMP_Text CreateText(Transform parent, string name, string text, int size, Vector2 min, Vector2 max, TextAlignmentOptions align, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        if (min != Vector2.zero || max != Vector2.zero)
            SetAnchors(go.GetComponent<RectTransform>(), min, max);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = color;
        return tmp;
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
}

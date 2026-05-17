using UnityEngine;

public static class NpcVisualSpawn
{
    public static GameObject Create(string npcName, GameObject prefab, string resourcePath, float fallbackNpcHeightMeters)
    {
        if (prefab != null)
            return Object.Instantiate(prefab);

        var texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.localScale = new Vector3(0.45f, fallbackNpcHeightMeters * 0.5f, 0.45f);
            AddTitleLabel(capsule.transform, npcName, fallbackNpcHeightMeters + 0.18f);
            return capsule;
        }

        return CreateSpriteNpc(npcName, texture, fallbackNpcHeightMeters, npcName);
    }

    /// <summary>
    /// PNG'yi olduğu gibi (alfa kanalı ile) gösterir; ekstra alpha/chroma işlemi yok.
    /// </summary>
    public static GameObject CreateOutdoorAr(string npcName, string resourcePath, float fallbackNpcHeightMeters, string displayTitle = null)
    {
        var texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
            return Create(npcName, null, resourcePath, fallbackNpcHeightMeters);

        var title = string.IsNullOrWhiteSpace(displayTitle) ? npcName : displayTitle;
        return NpcArBillboardVisual.Create(npcName, texture, fallbackNpcHeightMeters, null, title);
    }

    static GameObject CreateSpriteNpc(string objectName, Texture2D texture, float heightMeters, string title)
    {
        var npcObject = new GameObject(objectName);
        var renderer = npcObject.AddComponent<SpriteRenderer>();
        var pixelsPerUnit = texture.height / Mathf.Max(0.1f, heightMeters);
        renderer.sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0f),
            pixelsPerUnit);
        renderer.color = Color.white;
        renderer.sortingOrder = 100;

        var collider = npcObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.9f, heightMeters, 0.15f);
        collider.center = new Vector3(0f, heightMeters * 0.5f, 0f);

        npcObject.AddComponent<NpcWorldBillboard>();
        AddTitleLabel(npcObject.transform, title, heightMeters + 0.14f);
        return npcObject;
    }

    public static void AddTitleLabel(Transform parent, string title, float heightMeters)
    {
        if (parent == null || string.IsNullOrWhiteSpace(title))
            return;

        var existing = parent.Find("NpcTitle");
        if (existing != null)
            Object.Destroy(existing.gameObject);

        var labelRoot = new GameObject("NpcTitle");
        labelRoot.transform.SetParent(parent, false);
        labelRoot.transform.localPosition = new Vector3(0f, heightMeters, 0f);
        labelRoot.AddComponent<NpcWorldTextBillboard>();

        var shadow = new GameObject("Shadow");
        shadow.transform.SetParent(labelRoot.transform, false);
        shadow.transform.localPosition = new Vector3(0.03f, -0.03f, 0.02f);
        var shadowMesh = shadow.AddComponent<TextMesh>();
        ConfigureTitleTextMesh(shadowMesh, title, new Color(0f, 0f, 0f, 0.85f));

        var textObject = new GameObject("Text");
        textObject.transform.SetParent(labelRoot.transform, false);
        textObject.transform.localPosition = Vector3.zero;
        var textMesh = textObject.AddComponent<TextMesh>();
        ConfigureTitleTextMesh(textMesh, title, Color.white);
    }

    static void ConfigureTitleTextMesh(TextMesh textMesh, string title, Color color)
    {
        textMesh.text = title;
        textMesh.fontSize = 64;
        textMesh.characterSize = 0.028f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        textMesh.fontStyle = FontStyle.Bold;
    }
}

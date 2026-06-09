using UnityEngine;
using UnityEngine.UI;

public static class LocationExperienceUISprites
{
    static Sprite roundedRectSprite;
    static Sprite roundedTopPanelSprite;

    public static Sprite GetRoundedRectSprite(int width, int height, int radius)
    {
        if (roundedRectSprite != null)
            return roundedRectSprite;

        roundedRectSprite = CreateRoundedRectSprite(width, height, radius, true, true, true, true);
        return roundedRectSprite;
    }

    public static Sprite GetRoundedTopPanelSprite(int width, int height, int radius)
    {
        if (roundedTopPanelSprite != null)
            return roundedTopPanelSprite;

        roundedTopPanelSprite = CreateRoundedRectSprite(width, height, radius, true, true, false, false);
        return roundedTopPanelSprite;
    }

    public static void ApplyRoundedRect(Image image, Color color, int width, int height, int radius)
    {
        if (image == null)
            return;

        image.sprite = GetRoundedRectSprite(width, height, radius);
        image.type = UnityEngine.UI.Image.Type.Sliced;
        image.color = color;
        image.pixelsPerUnitMultiplier = 1f;
    }

    public static void ApplyRoundedTopPanel(Image image, Color color, int width, int height, int radius)
    {
        if (image == null)
            return;

        image.sprite = GetRoundedTopPanelSprite(width, height, radius);
        image.type = UnityEngine.UI.Image.Type.Sliced;
        image.color = color;
        image.pixelsPerUnitMultiplier = 1f;
    }

    static Sprite CreateRoundedRectSprite(int width, int height, int radius, bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        var pixels = new Color32[width * height];
        var r = Mathf.Clamp(radius, 1, Mathf.Min(width, height) / 2);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var inside = IsInsideRoundedPixel(x, y, width, height, r, topLeft, topRight, bottomLeft, bottomRight);
                pixels[y * width + x] = inside
                    ? new Color32(255, 255, 255, 255)
                    : new Color32(255, 255, 255, 0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        var border = r + 1;
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(border, border, border, border));
    }

    static bool IsInsideRoundedPixel(int x, int y, int width, int height, int radius, bool topLeft, bool topRight, bool bottomLeft, bool bottomRight)
    {
        if (topLeft && x < radius && y >= height - radius)
        {
            var dx = radius - x;
            var dy = y - (height - radius);
            return dx * dx + dy * dy <= radius * radius;
        }

        if (topRight && x >= width - radius && y >= height - radius)
        {
            var dx = x - (width - radius - 1);
            var dy = y - (height - radius);
            return dx * dx + dy * dy <= radius * radius;
        }

        if (bottomLeft && x < radius && y < radius)
        {
            var dx = radius - x;
            var dy = radius - y;
            return dx * dx + dy * dy <= radius * radius;
        }

        if (bottomRight && x >= width - radius && y < radius)
        {
            var dx = x - (width - radius - 1);
            var dy = radius - y;
            return dx * dx + dy * dy <= radius * radius;
        }

        return true;
    }
}

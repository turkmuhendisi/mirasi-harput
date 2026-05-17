using UnityEngine;

/// <summary>
/// Park NPC PNG'leri çoğu zaman siyah arka plan + tam opak alpha ile gelir.
/// Bu yardımcı, gerçek alpha yoksa arka planı şeffaf yapar.
/// </summary>
public static class NpcPngTextureUtility
{
    const int AlphaSampleStride = 4096;

    public static Texture2D PrepareForBillboard(Texture2D source, float tolerance = 0.14f, float softness = 0.07f)
    {
        if (source == null)
            return null;

        if (HasTransparentPixels(source))
            return source;

        return BuildAlphaFromBackground(source, tolerance, softness);
    }

    public static bool HasTransparentPixels(Texture2D texture)
    {
        if (texture == null || !texture.isReadable)
            return false;

        var pixels = texture.GetPixels32();
        if (pixels == null || pixels.Length == 0)
            return false;

        for (var i = 0; i < pixels.Length; i += AlphaSampleStride)
        {
            if (pixels[i].a < 250)
                return true;
        }

        return false;
    }

    public static Texture2D BuildAlphaFromBackground(Texture2D source, float tolerance, float softness)
    {
        if (source == null || !source.isReadable)
            return source;

        try
        {
            var w = source.width;
            var h = source.height;
            var pixels = source.GetPixels32();

            var c00 = (Color)pixels[0];
            var c10 = (Color)pixels[w - 1];
            var c01 = (Color)pixels[(h - 1) * w];
            var c11 = (Color)pixels[(h - 1) * w + (w - 1)];
            var bg = new Color(
                (c00.r + c10.r + c01.r + c11.r) * 0.25f,
                (c00.g + c10.g + c01.g + c11.g) * 0.25f,
                (c00.b + c10.b + c01.b + c11.b) * 0.25f,
                1f);

            var outPixels = new Color32[pixels.Length];
            var tolSq = tolerance * tolerance * 3f;
            var soft = Mathf.Max(0.0001f, softness * softness * 3f);

            for (var i = 0; i < pixels.Length; i++)
            {
                var p = (Color)pixels[i];
                var dr = p.r - bg.r;
                var dg = p.g - bg.g;
                var db = p.b - bg.b;
                var distSq = dr * dr + dg * dg + db * db;

                float alpha;
                if (distSq <= tolSq)
                    alpha = 0f;
                else
                    alpha = Mathf.Clamp01((distSq - tolSq) / soft);

                outPixels[i] = new Color32(pixels[i].r, pixels[i].g, pixels[i].b, (byte)(alpha * 255f));
            }

            var copy = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
            copy.name = source.name + "_Alpha";
            copy.wrapMode = TextureWrapMode.Clamp;
            copy.filterMode = source.filterMode;
            copy.SetPixels32(outPixels);
            copy.Apply(false, true);
            return copy;
        }
        catch
        {
            return source;
        }
    }
}

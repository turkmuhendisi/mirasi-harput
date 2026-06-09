using UnityEngine;
using ZXing;

public static class WebCamQrFrameProcessor
{
    static readonly RGBLuminanceSource.BitmapFormat[] DecodeFormats =
    {
        RGBLuminanceSource.BitmapFormat.RGBA32,
        RGBLuminanceSource.BitmapFormat.BGRA32,
        RGBLuminanceSource.BitmapFormat.ARGB32
    };

    public static bool TryDecode(BarcodeReaderGeneric reader, WebCamTexture webCam, Color32[] pixelBuffer, out string text)
    {
        text = null;
        if (reader == null || webCam == null || pixelBuffer == null || pixelBuffer.Length == 0)
            return false;

        var width = webCam.width;
        var height = webCam.height;
        if (width <= 16 || height <= 16)
            return false;

        webCam.GetPixels32(pixelBuffer);

        var rotation = webCam.videoRotationAngle;
        var mirrored = webCam.videoVerticallyMirrored;

        if (TryDecodeOrientation(reader, pixelBuffer, width, height, rotation, mirrored, out text))
            return true;

        if (rotation != 0 && TryDecodeOrientation(reader, pixelBuffer, width, height, 0, mirrored, out text))
            return true;

        return false;
    }

    static bool TryDecodeOrientation(
        BarcodeReaderGeneric reader,
        Color32[] source,
        int width,
        int height,
        int rotationAngle,
        bool verticallyMirrored,
        out string text)
    {
        text = null;
        var oriented = BuildOrientedPixels(source, width, height, rotationAngle, verticallyMirrored, out var decodeWidth, out var decodeHeight);
        if (oriented == null || oriented.Length == 0)
            return false;

        var byteLength = oriented.Length * 4;
        var rgbaBytes = new byte[byteLength];
        System.Buffer.BlockCopy(oriented, 0, rgbaBytes, 0, byteLength);

        for (var i = 0; i < DecodeFormats.Length; i++)
        {
            var result = reader.Decode(rgbaBytes, decodeWidth, decodeHeight, DecodeFormats[i]);
            if (result == null || string.IsNullOrWhiteSpace(result.Text))
                continue;

            text = result.Text;
            return true;
        }

        return false;
    }

    static Color32[] BuildOrientedPixels(
        Color32[] source,
        int width,
        int height,
        int rotationAngle,
        bool verticallyMirrored,
        out int outWidth,
        out int outHeight)
    {
        var normalizedAngle = ((rotationAngle % 360) + 360) % 360;
        var rotated = normalizedAngle == 90 || normalizedAngle == 270
            ? Rotate90(source, width, height, normalizedAngle == 90)
            : source;

        var rotatedWidth = normalizedAngle == 90 || normalizedAngle == 270 ? height : width;
        var rotatedHeight = normalizedAngle == 90 || normalizedAngle == 270 ? width : height;

        if (verticallyMirrored)
            rotated = MirrorVertical(rotated, rotatedWidth, rotatedHeight);

        outWidth = rotatedWidth;
        outHeight = rotatedHeight;
        return rotated;
    }

    static Color32[] Rotate90(Color32[] source, int width, int height, bool clockwise)
    {
        var rotated = new Color32[source.Length];
        var newWidth = height;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var sourceIndex = y * width + x;
                int destX;
                int destY;

                if (clockwise)
                {
                    destX = height - 1 - y;
                    destY = x;
                }
                else
                {
                    destX = y;
                    destY = width - 1 - x;
                }

                rotated[destY * newWidth + destX] = source[sourceIndex];
            }
        }

        return rotated;
    }

    static Color32[] MirrorVertical(Color32[] source, int width, int height)
    {
        var mirrored = new Color32[source.Length];

        for (var y = 0; y < height; y++)
        {
            var destY = height - 1 - y;
            for (var x = 0; x < width; x++)
            {
                var sourceIndex = y * width + x;
                var destIndex = destY * width + x;
                mirrored[destIndex] = source[sourceIndex];
            }
        }

        return mirrored;
    }
}

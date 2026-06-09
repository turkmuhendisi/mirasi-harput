using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;

public static class ArCameraQrFrameProcessor
{
    public static bool TryDecode(
        BarcodeReaderGeneric reader,
        ARCameraManager cameraManager,
        ref byte[] rgbaByteBuffer,
        out string text)
    {
        text = null;
        if (reader == null || cameraManager == null)
            return false;

        if (!cameraManager.TryAcquireLatestCpuImage(out var cpuImage))
            return false;

        try
        {
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
                outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorY
            };

            var requiredBytes = cpuImage.GetConvertedDataSize(conversionParams);
            if (rgbaByteBuffer == null || rgbaByteBuffer.Length != requiredBytes)
                rgbaByteBuffer = new byte[requiredBytes];

            var nativeBuffer = new NativeArray<byte>(requiredBytes, Allocator.Temp);
            try
            {
                cpuImage.Convert(conversionParams, nativeBuffer);
                nativeBuffer.CopyTo(rgbaByteBuffer);
            }
            finally
            {
                nativeBuffer.Dispose();
            }

            var result = reader.Decode(
                rgbaByteBuffer,
                conversionParams.outputDimensions.x,
                conversionParams.outputDimensions.y,
                RGBLuminanceSource.BitmapFormat.RGBA32);

            if (result == null || string.IsNullOrWhiteSpace(result.Text))
                return false;

            text = result.Text;
            return true;
        }
        finally
        {
            cpuImage.Dispose();
        }
    }
}

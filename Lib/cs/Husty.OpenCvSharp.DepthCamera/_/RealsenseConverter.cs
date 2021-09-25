using OpenCvSharp;
using Intel.RealSense;

namespace Husty.OpenCvSharp.DepthCamera
{
    /// <summary>
    /// Native API type -> OpenCvSharp format
    /// </summary>
    internal static class RealsenseConverter
    {

        internal unsafe static Mat ToColorMat(this VideoFrame frame)
        {
            var colorMat = new Mat(frame.Height, frame.Width, MatType.CV_8UC3);
            var rgbData = (byte*)frame.Data;
            var pixels = colorMat.DataPointer;
            for (int i = 0; i < colorMat.Width * colorMat.Height; i++)
            {
                pixels[i * 3 + 0] = rgbData[i * 3 + 2];
                pixels[i * 3 + 1] = rgbData[i * 3 + 1];
                pixels[i * 3 + 2] = rgbData[i * 3 + 0];
            }
            return colorMat;
        }

        internal unsafe static Mat ToPointCloudMat(this Frame frame, int width, int height)
        {
            var pointCloudMat = new Mat(height, width, MatType.CV_16UC3);
            using var pdFrame = new PointCloud().Process(frame);
            var pData = (float*)(pdFrame.Data);
            var pixels = (ushort*)pointCloudMat.Data;
            int index = 0;
            for (int i = 0; i < pointCloudMat.Width * pointCloudMat.Height; i++)
            {
                pixels[index] = (ushort)(pData[index++] * 1000);
                pixels[index] = (ushort)(pData[index++] * 1000);
                pixels[index] = (ushort)(pData[index++] * 1000);
            }
            return pointCloudMat;
        }

    }
}

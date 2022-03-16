using OpenCvSharp;
using Intel.RealSense;

namespace Husty.OpenCvSharp.DepthCamera
{
    /// <summary>
    /// Native API type -> OpenCvSharp format
    /// </summary>
    internal static class RealsenseEx
    {

        internal unsafe static void CopyToColorMat(this VideoFrame frame, Mat colorMat)
        {
            if (colorMat.IsDisposed || colorMat.Width != frame.Width || colorMat.Height != frame.Height || colorMat.Type() != MatType.CV_8UC3)
                return;
            frame.CopyTo(colorMat.Data);
            Cv2.CvtColor(colorMat, colorMat, ColorConversionCodes.RGB2BGR);
        }

        internal unsafe static void CopyToPointCloudMat(this Frame frame, Mat pointCloudMat, int width, int height)
        {
            if (pointCloudMat.IsDisposed || pointCloudMat.Width != width || pointCloudMat.Height != height || pointCloudMat.Type() != MatType.CV_16UC3)
                return;
            using var pdFrame0 = new PointCloud();
            using var pdFrame1 = pdFrame0.Process(frame);
            var pData = (float*)(pdFrame1.Data);
            var pixels = (ushort*)pointCloudMat.Data;
            int index = 0;
            for (int i = 0; i < pointCloudMat.Width * pointCloudMat.Height; i++)
            {
                pixels[index] = (ushort)(pData[index++] * 1000);
                pixels[index] = (ushort)(pData[index++] * 1000);
                pixels[index] = (ushort)(pData[index++] * 1000);
            }
        }

    }
}

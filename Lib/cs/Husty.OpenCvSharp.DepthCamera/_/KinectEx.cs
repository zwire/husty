using OpenCvSharp;
using Microsoft.Azure.Kinect.Sensor;

namespace Husty.OpenCvSharp.DepthCamera
{
    /// <summary>
    /// Native API type -> OpenCvSharp format
    /// </summary>
    internal static class KinectEx
    {

        internal unsafe static void CopyToColorMat(this Image colorFrame, Mat colorMat)
        {
            var w = colorFrame.WidthPixels;
            var h = colorFrame.HeightPixels;
            if (colorMat.IsDisposed || colorMat.Width != w || colorMat.Height != h || colorMat.Type() != MatType.CV_8UC3)
                return;
            var m = colorFrame.Memory;
            var cAry = colorFrame.GetPixels<BGRA>().Span;
            var p = colorMat.DataPointer;
            int index = 0;
            for (int i = 0; i < cAry.Length; i++)
            {
                p[index++] = cAry[i].B;
                p[index++] = cAry[i].G;
                p[index++] = cAry[i].R;
            }
        }

        internal unsafe static void CopyToPointCloudMat(this Image pointCloudFrame, Mat pointCloudMat)
        {
            var w = pointCloudFrame.WidthPixels;
            var h = pointCloudFrame.HeightPixels;
            if (pointCloudMat.IsDisposed || pointCloudMat.Width != w || pointCloudMat.Height != h || pointCloudMat.Type() != MatType.CV_16UC3)
                return;
            var pdAry = pointCloudFrame.GetPixels<Short3>().Span;
            var p = (ushort*)pointCloudMat.Data;
            int index = 0;
            for (int i = 0; i < pdAry.Length; i++)
            {
                p[index++] = (ushort)pdAry[i].X;
                p[index++] = (ushort)pdAry[i].Y;
                p[index++] = (ushort)pdAry[i].Z;
            }
        }

    }
}

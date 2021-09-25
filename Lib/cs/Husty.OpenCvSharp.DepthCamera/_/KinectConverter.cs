using OpenCvSharp;
using Microsoft.Azure.Kinect.Sensor;

namespace Husty.OpenCvSharp.DepthCamera
{
    /// <summary>
    /// Native API type -> OpenCvSharp format
    /// </summary>
    internal static class KinectConverter
    {

        internal unsafe static Mat ToColorMat(this Image colorFrame)
        {
            var w = colorFrame.WidthPixels;
            var h = colorFrame.HeightPixels;
            var colorMat = new Mat(h, w, MatType.CV_8UC3);
            var cAry = colorFrame.GetPixels<BGRA>().ToArray();
            var p = colorMat.DataPointer;
            int index = 0;
            for (int i = 0; i < cAry.Length; i++)
            {
                p[index++] = cAry[i].B;
                p[index++] = cAry[i].G;
                p[index++] = cAry[i].R;
            }
            return colorMat;
        }

        internal unsafe static Mat ToPointCloudMat(this Image pointCloudFrame)
        {
            var w = pointCloudFrame.WidthPixels;
            var h = pointCloudFrame.HeightPixels;
            var pointCloudMat = new Mat(h, w, MatType.CV_16UC3);
            var pdAry = pointCloudFrame.GetPixels<Short3>().ToArray();
            var p = (ushort*)pointCloudMat.Data;
            int index = 0;
            for (int i = 0; i < pdAry.Length; i++)
            {
                p[index++] = (ushort)pdAry[i].X;
                p[index++] = (ushort)pdAry[i].Y;
                p[index++] = (ushort)pdAry[i].Z;
            }
            return pointCloudMat;
        }

    }
}

using OpenCvSharp;
using Microsoft.Azure.Kinect.Sensor;

namespace Husty.OpenCvSharp.DepthCamera
{
    /// <summary>
    /// Native API type -> OpenCvSharp format
    /// </summary>
    internal class KinectConverter
    {

        // ------- Fields ------- //

        private readonly int _width;
        private readonly int _height;


        // ------- Constructor ------- //

        internal KinectConverter(int width, int height)
        {
            _width = width;
            _height = height;
        }


        // ------- Methods ------- //

        internal unsafe void ToColorMat(Image colorImg, ref Mat colorMat)
        {
            if (colorMat.Type() != MatType.CV_8UC3) colorMat = new Mat(_height, _width, MatType.CV_8UC3);
            var cAry = colorImg.GetPixels<BGRA>().ToArray();
            var p = colorMat.DataPointer;
            int index = 0;
            for (int i = 0; i < cAry.Length; i++)
            {
                p[index++] = cAry[i].B;
                p[index++] = cAry[i].G;
                p[index++] = cAry[i].R;
            }
        }

        internal unsafe void ToPointCloudMat(Image pointCloudImg, ref Mat pointCloudMat)
        {
            if (pointCloudMat.Type() != MatType.CV_16UC3) pointCloudMat = new Mat(_height, _width, MatType.CV_16UC3);
            var pdAry = pointCloudImg.GetPixels<Short3>().ToArray();
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

using System;
using OpenCvSharp;

namespace Husty.OpenCvSharp.DepthCamera
{

    /// <summary>
    /// Point Cloud with Color
    /// </summary>
    public class BgrXyzMat : IDisposable
    {

        // ------- Properties ------- //

        /// <summary>
        /// Color image
        /// </summary>
        public Mat BGR { get; }

        /// <summary>
        /// Point Cloud image
        /// </summary>
        public Mat XYZ { get; }

        /// <summary>
        /// Depth image (0-65535)(mm)
        /// </summary>
        public Mat Depth16 => XYZ.Split()[2];

        public int Width => BGR.Width;

        public int Height => BGR.Height;

        public int Rows => BGR.Rows;

        public int Cols => BGR.Cols;


        // ------- Constructor ------- //

        /// <summary>
        /// Create empty instance.
        /// </summary>
        public BgrXyzMat()
        {
            BGR = new();
            XYZ = new();
        }

        /// <summary>
        /// Hold Point Cloud with Color.
        /// </summary>
        /// <param name="bgr">Color image</param>
        /// <param name="xyz">Point Cloud image (must be same size of Color image)</param>
        public BgrXyzMat(Mat bgr, Mat xyz)
        {
            BGR = bgr;
            XYZ = xyz;
        }

        /// <summary>
        /// Decode from byte array.
        /// </summary>
        /// <param name="BGRBytes"></param>
        /// <param name="XYZBytes"></param>
        public BgrXyzMat(byte[] BGRBytes, byte[] XYZBytes)
        {
            BGR = Cv2.ImDecode(BGRBytes, ImreadModes.Unchanged);
            XYZ = Cv2.ImDecode(XYZBytes, ImreadModes.Unchanged);
            if (BGR.Width != XYZ.Width || BGR.Height != XYZ.Height)
                throw new InvalidOperationException("Require: BGR size == XYZ size");
        }


        // ------- Methods ------- //


        public void Dispose()
        {
            BGR?.Dispose();
            XYZ?.Dispose();
        }

        /// <summary>
        /// Same function as constructor.
        /// </summary>
        /// <param name="bgr">Color Image</param>
        /// <param name="xyz">Point Cloud Image (must be same size of Color Image)</param>
        /// <returns></returns>
        public static BgrXyzMat Create(Mat bgr, Mat xyz)
            => new(bgr, xyz);

        /// <summary>
        /// Decode from byte array.
        /// </summary>
        /// <param name="BGRBytes"></param>
        /// <param name="XYZBytes"></param>
        /// <returns></returns>
        public static BgrXyzMat YmsDecode(byte[] BGRBytes, byte[] XYZBytes)
            => new(Cv2.ImDecode(BGRBytes, ImreadModes.Unchanged), Cv2.ImDecode(XYZBytes, ImreadModes.Unchanged));

        /// <summary>
        /// Output encoded byte array.
        /// </summary>
        /// <returns></returns>
        public (byte[] BGRBytes, byte[] XYZBytes) YmsEncode()
            => (BGR.ImEncode(), XYZ.ImEncode());

        /// <summary>
        /// Check if it's empty.
        /// </summary>
        /// <returns></returns>
        public bool Empty() => BGR.Empty() || XYZ.Empty();

        /// <summary>
        /// Create deep copy of this object.
        /// </summary>
        /// <returns></returns>
        public BgrXyzMat Clone() => new(BGR.Clone(), XYZ.Clone());

        public BgrXyzMat Resize(Size size)
        {
            Cv2.Resize(BGR, BGR, size);
            Cv2.Resize(XYZ, XYZ, size);
            return this;
        }

        /// <summary>
        /// Get Depth image (Normalize value in 0-255)
        /// </summary>
        /// <param name="minDistance">(mm)</param>
        /// <param name="maxDistance">(mm)</param>
        /// <returns></returns>
        public unsafe Mat Depth8(int minDistance, int maxDistance)
        {
            var depth8 = new Mat(XYZ.Height, XYZ.Width, MatType.CV_8U);
            var d8 = depth8.DataPointer;
            var d16 = (ushort*)Depth16.Data;
            for (int j = 0; j < XYZ.Width * XYZ.Height; j++)
            {
                if (d16[j] < 300) d8[j] = 0;
                else if (d16[j] < maxDistance) d8[j] = (byte)((d16[j] - minDistance) * 255 / (maxDistance - minDistance));
                else d8[j] = 255;
            }
            return depth8;
        }

        /// <summary>
        /// Get information of where you input.
        /// </summary>
        /// <param name="point">Pixel Coordinate</param>
        /// <returns>Real 3D coordinate with color</returns>
        public unsafe BGRXYZ GetPointInfo(Point point)
        {
            if (BGR.Width != XYZ.Width || BGR.Height != XYZ.Height)
                throw new InvalidOperationException("Require: BGR size == XYZ size");
            var index = (point.Y * BGR.Cols + point.X) * 3;
            var bgr = BGR.DataPointer;
            var xyz = (short*)XYZ.Data;
            var b = bgr[index + 0];
            var g = bgr[index + 1];
            var r = bgr[index + 2];
            var x = xyz[index + 0];
            var y = xyz[index + 1];
            var z = xyz[index + 2];
            return new BGRXYZ(b, g, r, x, y, z);
        }

        /// <summary>
        /// Move all Point Cloud.
        /// </summary>
        /// <param name="delta">3D vector of translation (mm)</param>
        public unsafe BgrXyzMat Move(Vec3s delta)
        {
            var s = (short*)XYZ.Data;
            var index = 0;
            for (int i = 0; i < XYZ.Rows * XYZ.Cols; i++)
            {
                s[index++] += delta.Item0;
                s[index++] += delta.Item1;
                s[index++] += delta.Item2;
            }
            return this;
        }

        /// <summary>
        /// Change Point Cloud scale.
        /// </summary>
        /// <param name="delta">Scale of XYZ direction</param>
        public unsafe BgrXyzMat Scale(Vec3s delta)
        {
            var s = (short*)XYZ.Data;
            var index = 0;
            for (int i = 0; i < XYZ.Rows * XYZ.Cols; i++)
            {
                s[index++] *= delta.Item0;
                s[index++] *= delta.Item1;
                s[index++] *= delta.Item2;
            }
            return this;
        }

        /// <summary>
        /// Rotate 3D of right hand system.
        /// </summary>
        /// <param name="pitch">Pitch angle (rad, clockwise of X axis)</param>
        /// <param name="yaw">Yaw angle (rad, clockwise of Y axis)</param>
        /// <param name="roll">Roll angle (rad, clockwise of Z axis)</param>
        public unsafe BgrXyzMat Rotate(float pitch, float yaw, float roll)
        {
            return Rotate(ZRot(roll) * YRot(yaw) * XRot(pitch));
        }

        /// <summary>
        /// Rotate 3D
        /// </summary>
        /// <param name="rotationMat">Rotation Matrix</param>
        /// <returns></returns>
        public unsafe BgrXyzMat Rotate(Mat rotationMat)
        {
            var d = (float*)rotationMat.Data;
            var s = (short*)XYZ.Data;
            for (int i = 0; i < XYZ.Rows * XYZ.Cols * 3; i += 3)
            {
                var x = s[i + 0];
                var y = s[i + 1];
                var z = s[i + 2];
                s[i + 0] = (short)(d[0] * x + d[1] * y + d[2] * z);
                s[i + 1] = (short)(d[3] * x + d[4] * y + d[5] * z);
                s[i + 2] = (short)(d[6] * x + d[7] * y + d[8] * z);
            }
            return this;
        }

        private static Mat XRot(float rad)
            => new(3, 3, MatType.CV_32F, new float[] { 1, 0, 0, 0, (float)Math.Cos(rad), -(float)Math.Sin(rad), 0, (float)Math.Sin(rad), (float)Math.Cos(rad) });

        private static Mat YRot(float rad)
            => new(3, 3, MatType.CV_32F, new float[] { (float)Math.Cos(rad), 0, (float)Math.Sin(rad), 0, 1, 0, -(float)Math.Sin(rad), 0, (float)Math.Cos(rad) });

        private static Mat ZRot(float rad)
            => new(3, 3, MatType.CV_32F, new float[] { (float)Math.Cos(rad), -(float)Math.Sin(rad), 0, (float)Math.Sin(rad), (float)Math.Cos(rad), 0, 0, 0, 1 });


    }


    /// <summary>
    /// Record of Point and Color
    /// </summary>
    public record BGRXYZ(byte B, byte G, byte R, short X, short Y, short Z)
    {

        public Vec3s Vector3 => new(X, Y, Z);

    }

}

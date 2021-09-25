using System;
using OpenCvSharp;

namespace Husty.OpenCvSharp.DepthCamera
{
    /// <summary>
    /// For using Point Cloud image
    /// </summary>
    public record Vector3(short X, short Y, short Z)
    {

        // ------- Methods ------- //

        public int GetLength()
            => (int)Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vector3 GetUnitVec()
            => this / GetLength();

        public Vector3 Invert()
            => new((short)-X, (short)-Y, (short)-Z);

        /// <summary>
        /// Move this vector
        /// </summary>
        /// <param name="delta">3D vector of transform (mm)</param>
        public Vector3 Move(Vector3 delta)
        {
            var x = (short)(X + delta.X);
            var y = (short)(Y + delta.Y);
            var z = (short)(Z + delta.Z);
            return new(x, y, z);
        }

        /// <summary>
        /// Change scale.
        /// </summary>
        /// <param name="delta">Scale of XYZ direction</param>
        public Vector3 Scale(Vector3 delta)
        {
            var x = (short)(X * delta.X);
            var y = (short)(Y * delta.Y);
            var z = (short)(Z * delta.Z);
            return new(x, y, z);
        }

        /// <summary>
        /// Rotate 3D of right hand system.
        /// </summary>
        /// <param name="pitch">Pitch angle (rad, clockwise of X axis)</param>
        /// <param name="yaw">Yaw angle (rad, clockwise of Y axis)</param>
        /// <param name="roll">Roll angle (rad, clockwise of Z axis)</param>
        public unsafe Vector3 Rotate(float pitch, float yaw, float roll)
        {
            Mat rot = ZRot(roll) * YRot(yaw) * XRot(pitch);
            var d = (float*)rot.Data;
            var x = (short)(d[0] * X + d[1] * Y + d[2] * Z);
            var y = (short)(d[3] * X + d[4] * Y + d[5] * Z);
            var z = (short)(d[6] * X + d[7] * Y + d[8] * Z);
            return new(x, y, z);
        }

        /// <summary>
        /// Rotate 3D
        /// </summary>
        /// <param name="rotationMat">Rotation Matrix</param>
        /// <returns></returns>
        public unsafe Vector3 Rotate(Mat rotationMat)
        {
            var d = (float*)rotationMat.Data;
            var x = (short)(d[0] * X + d[1] * Y + d[2] * Z);
            var y = (short)(d[3] * X + d[4] * Y + d[5] * Z);
            var z = (short)(d[6] * X + d[7] * Y + d[8] * Z);
            return new(x, y, z);
        }

        internal static Mat XRot(float rad)
            => new(3, 3, MatType.CV_32F, new float[] { 1, 0, 0, 0, (float)Math.Cos(rad), -(float)Math.Sin(rad), 0, (float)Math.Sin(rad), (float)Math.Cos(rad) });

        internal static Mat YRot(float rad)
            => new(3, 3, MatType.CV_32F, new float[] { (float)Math.Cos(rad), 0, (float)Math.Sin(rad), 0, 1, 0, -(float)Math.Sin(rad), 0, (float)Math.Cos(rad) });

        internal static Mat ZRot(float rad)
            => new(3, 3, MatType.CV_32F, new float[] { (float)Math.Cos(rad), -(float)Math.Sin(rad), 0, (float)Math.Sin(rad), (float)Math.Cos(rad), 0, 0, 0, 1 });


        // ------- Operators ------- //

        public static Vector3 operator +(Vector3 vec1, Vector3 vec2)
            => new((short)(vec2.X + vec1.X), (short)(vec2.Y + vec1.Y), (short)(vec2.Z + vec1.Z));

        public static Vector3 operator -(Vector3 vec1, Vector3 vec2)
            => new((short)(vec2.X - vec1.X), (short)(vec2.Y - vec1.Y), (short)(vec2.Z - vec1.Z));

        public static Vector3 operator *(Vector3 vec, double scalar)
            => new((short)(vec.X * scalar), (short)(vec.Y * scalar), (short)(vec.Z * scalar));

        public static Vector3 operator /(Vector3 vec, double scalar)
            => new((short)(vec.X / scalar), (short)(vec.Y / scalar), (short)(vec.Z / scalar));

    }
}

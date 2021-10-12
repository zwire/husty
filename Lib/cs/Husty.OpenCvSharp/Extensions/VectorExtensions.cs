using OpenCvSharp;
using static System.Math;

namespace Husty.OpenCvSharp
{

    public static class Vector
    {

        public static Vec2b CreateVec2b(Point2d p1, Point2d p2)
        {
            return new((byte)(p2.X - p1.X), (byte)(p2.Y - p1.Y));
        }

        public static Vec2w CreateVec2w(Point2d p1, Point2d p2)
        {
            return new((ushort)(p2.X - p1.X), (ushort)(p2.Y - p1.Y));
        }

        public static Vec2s CreateVec2s(Point2d p1, Point2d p2)
        {
            return new((short)(p2.X - p1.X), (short)(p2.Y - p1.Y));
        }

        public static Vec2i CreateVec2i(Point2d p1, Point2d p2)
        {
            return new((int)(p2.X - p1.X), (int)(p2.Y - p1.Y));
        }

        public static Vec2f CreateVec2f(Point2d p1, Point2d p2)
        {
            return new((float)(p2.X - p1.X), (float)(p2.Y - p1.Y));
        }

        public static Vec2d CreateVec2d(Point2d p1, Point2d p2)
        {
            return new(p2.X - p1.X, p2.Y - p1.Y);
        }

        public static Vec3b CreateVec3b(Point3d p1, Point3d p2)
        {
            return new((byte)(p2.X - p1.X), (byte)(p2.Y - p1.Y), (byte)(p2.Z - p1.Z));
        }

        public static Vec3w CreateVec3w(Point3d p1, Point3d p2)
        {
            return new((ushort)(p2.X - p1.X), (ushort)(p2.Y - p1.Y), (ushort)(p2.Z - p1.Z));
        }

        public static Vec3s CreateVec3s(Point3d p1, Point3d p2)
        {
            return new((short)(p2.X - p1.X), (short)(p2.Y - p1.Y), (short)(p2.Z - p1.Z));
        }

        public static Vec3i CreateVec3i(Point3d p1, Point3d p2)
        {
            return new((int)(p2.X - p1.X), (int)(p2.Y - p1.Y), (int)(p2.Z - p1.Z));
        }

        public static Vec3f CreateVec3f(Point3d p1, Point3d p2)
        {
            return new((float)(p2.X - p1.X), (float)(p2.Y - p1.Y), (float)(p2.Z - p1.Z));
        }

        public static Vec3d CreateVec3d(Point3d p1, Point3d p2)
        {
            return new(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
        }

    }

    public static class VectorExtensions
    {

        // ------------------------- //

        public static double Norm(this Vec2b vec)
        {
            return vec.Item0 * vec.Item0 + vec.Item1 * vec.Item1;
        }

        public static double Norm(this Vec2w vec)
        {
            return vec.Item0 * vec.Item0 + vec.Item1 * vec.Item1;
        }

        public static double Norm(this Vec2s vec)
        {
            return vec.Item0 * vec.Item0 + vec.Item1 * vec.Item1;
        }

        public static double Norm(this Vec2i vec)
        {
            return vec.Item0 * vec.Item0 + vec.Item1 * vec.Item1;
        }

        public static double Norm(this Vec2f vec)
        {
            return vec.Item0 * vec.Item0 + vec.Item1 * vec.Item1;
        }

        public static double Norm(this Vec2d vec)
        {
            return vec.Item0 * vec.Item0 + vec.Item1 * vec.Item1;
        }

        public static double Norm(this Vec3b vec)
        {
            return vec.Item0 * vec.Item0 + vec.Item1 * vec.Item1 + vec.Item2 * vec.Item2;
        }

        public static double Norm(this Vec3w vec)
        {
            return vec.Item0 * vec.Item0 + vec.Item1 * vec.Item1 + vec.Item2 * vec.Item2;
        }

        public static double Norm(this Vec3s vec)
        {
            return vec.Item0 * vec.Item0 + vec.Item1 * vec.Item1 + vec.Item2 * vec.Item2;
        }

        public static double Norm(this Vec3i vec)
        {
            return vec.Item0 * vec.Item0 + vec.Item1 * vec.Item1 + vec.Item2 * vec.Item2;
        }

        public static double Norm(this Vec3f vec)
        {
            return vec.Item0 * vec.Item0 + vec.Item1 * vec.Item1 + vec.Item2 * vec.Item2;
        }

        public static double Norm(this Vec3d vec)
        {
            return vec.Item0 * vec.Item0 + vec.Item1 * vec.Item1 + vec.Item2 * vec.Item2;
        }

        // ------------------------- //

        public static Vec2b GetUnitVec(this Vec2b vec)
        {
            return vec / vec.Norm();
        }

        public static Vec2w GetUnitVec(this Vec2w vec)
        {
            return vec / vec.Norm();
        }

        public static Vec2s GetUnitVec(this Vec2s vec)
        {
            return vec / vec.Norm();
        }

        public static Vec2i GetUnitVec(this Vec2i vec)
        {
            return vec / vec.Norm();
        }

        public static Vec2f GetUnitVec(this Vec2f vec)
        {
            return vec / vec.Norm();
        }

        public static Vec2d GetUnitVec(this Vec2d vec)
        {
            return vec / vec.Norm();
        }

        public static Vec3b GetUnitVec(this Vec3b vec)
        {
            return vec / vec.Norm();
        }

        public static Vec3w GetUnitVec(this Vec3w vec)
        {
            return vec / vec.Norm();
        }

        public static Vec3s GetUnitVec(this Vec3s vec)
        {
            return vec / vec.Norm();
        }

        public static Vec3i GetUnitVec(this Vec3i vec)
        {
            return vec / vec.Norm();
        }

        public static Vec3f GetUnitVec(this Vec3f vec)
        {
            return vec / vec.Norm();
        }

        public static Vec3d GetUnitVec(this Vec3d vec)
        {
            return vec / vec.Norm();
        }

        // ------------------------- //

        public static Point ToPoint(this Vec2b vec)
        {
            return new(vec.Item0, vec.Item1);
        }

        public static Point ToPoint(this Vec2w vec)
        {
            return new(vec.Item0, vec.Item1);
        }

        public static Point ToPoint(this Vec2s vec)
        {
            return new(vec.Item0, vec.Item1);
        }

        public static Point ToPoint(this Vec2i vec)
        {
            return new(vec.Item0, vec.Item1);
        }

        public static Point2f ToPoint2f(this Vec2f vec)
        {
            return new(vec.Item0, vec.Item1);
        }

        public static Point2d ToPoint2d(this Vec2d vec)
        {
            return new(vec.Item0, vec.Item1);
        }

        public static Point3i ToPoint3i(this Vec3b vec)
        {
            return new(vec.Item0, vec.Item1, vec.Item2);
        }

        public static Point3i ToPoint3i(this Vec3w vec)
        {
            return new(vec.Item0, vec.Item1, vec.Item2);
        }

        public static Point3i ToPoint3i(this Vec3s vec)
        {
            return new(vec.Item0, vec.Item1, vec.Item2);
        }

        public static Point3i ToPoint3i(this Vec3i vec)
        {
            return new(vec.Item0, vec.Item1, vec.Item2);
        }

        public static Point3f ToPoint3f(this Vec3f vec)
        {
            return new(vec.Item0, vec.Item1, vec.Item2);
        }

        public static Point3d ToPoint3d(this Vec3d vec)
        {
            return new(vec.Item0, vec.Item1, vec.Item2);
        }

        // ------------------------- //

        public static Vec2s Rotate(this Vec2s vec, double radian)
        {
            var x0 = vec.Item0;
            var y0 = vec.Item1;
            var x = (short)(Cos(radian) * x0 - Sin(radian) * y0);
            var y = (short)(Sin(radian) * x0 + Cos(radian) * y0);
            return new(x, y);
        }

        public static Vec2i Rotate(this Vec2i vec, double radian)
        {
            var x0 = vec.Item0;
            var y0 = vec.Item1;
            var x = (int)(Cos(radian) * x0 - Sin(radian) * y0);
            var y = (int)(Sin(radian) * x0 + Cos(radian) * y0);
            return new(x, y);
        }

        public static Vec2f Rotate(this Vec2f vec, double radian)
        {
            var x0 = vec.Item0;
            var y0 = vec.Item1;
            var x = (float)(Cos(radian) * x0 - Sin(radian) * y0);
            var y = (float)(Sin(radian) * x0 + Cos(radian) * y0);
            return new(x, y);
        }

        public static Vec2d Rotate(this Vec2d vec, double radian)
        {
            var x0 = vec.Item0;
            var y0 = vec.Item1;
            var x = Cos(radian) * x0 - Sin(radian) * y0;
            var y = Sin(radian) * x0 + Cos(radian) * y0;
            return new(x, y);
        }

    }
}

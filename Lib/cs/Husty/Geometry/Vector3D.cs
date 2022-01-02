using static System.Math;

namespace Husty
{
    public class Vector3D
    {

        // ------ properties ------ //

        public double X { get; }

        public double Y { get; }

        public double Z { get; }

        public double Length => Sqrt(X * X + Y * Y + Z * Z) + 1e-15;

        public Vector3D UnitVector => this / Length;


        // ------ constructors ------ //

        public Vector3D(double x, double y, double z) { X = x; Y = y; Z = z; }

        public Vector3D(Point3D p1, Point3D p2) { X = p2.X - p1.X; Y = p2.Y - p1.Y; Z = p2.Z - p1.Z; }


        // ------ public methods ------ //

        public double[] ToArray() => new[] { X, Y, Z };


        // ------ operators ------ //

        public static Vector3D operator +(Vector3D v1, Vector3D v2) => new(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);

        public static Vector3D operator -(Vector3D v1, Vector3D v2) => new(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);

        public static Vector3D operator -(Vector3D v) => new(-v.X, -v.Y, -v.Z);

        public static Vector3D operator *(Vector3D v, double scalar) => new(v.X * scalar, v.Y * scalar, v.Z * scalar);

        public static Vector3D operator *(double scalar, Vector3D v) => v * scalar;

        public static Vector3D operator /(Vector3D v, double scalar) => new(v.X / scalar, v.Y / scalar, v.Z / scalar);

    }
}

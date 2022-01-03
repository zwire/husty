using static System.Math;

namespace Husty
{

    public enum Axis { X, Y, Z }

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

        public Angle GetAngle(Axis axis)
        {
            if (axis is Axis.Z)
                return new(Atan2(X, Y), AngleType.Radian);
            else if (axis is Axis.Y)
                return new(Atan2(Z, X), AngleType.Radian);
            else
                return new(Atan2(Y, Z), AngleType.Radian);
        }

        public Angle GetAngleFrom(Vector3D v, Axis axis)
        {
            return GetAngle(axis) - v.GetAngle(axis);
        }

        public Vector3D Rotate(Angle angle, Axis axis)
        {
            double x, y, z;
            if (axis is Axis.Z)
            {
                x = X * Cos(angle.Radian) - Y * Sin(angle.Radian);
                y = X * Sin(angle.Radian) + Y * Cos(angle.Radian);
                z = Z;
            }
            else if (axis is Axis.Y)
            {
                x = X * Cos(angle.Radian) + Z * Sin(angle.Radian);
                y = Y;
                z = -X * Sin(angle.Radian) + Z * Cos(angle.Radian);
            }
            else
            {
                x = X;
                y = Y * Cos(angle.Radian) - Z * Sin(angle.Radian);
                z = Y * Sin(angle.Radian) + Z * Cos(angle.Radian);
            }
            return new(x, y, z);
        }


        // ------ operators ------ //

        public static Vector3D operator +(Vector3D v1, Vector3D v2) => new(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);

        public static Vector3D operator -(Vector3D v1, Vector3D v2) => new(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);

        public static Vector3D operator -(Vector3D v) => new(-v.X, -v.Y, -v.Z);

        public static Vector3D operator *(Vector3D v, double scalar) => new(v.X * scalar, v.Y * scalar, v.Z * scalar);

        public static Vector3D operator *(double scalar, Vector3D v) => v * scalar;

        public static Vector3D operator /(Vector3D v, double scalar) => new(v.X / scalar, v.Y / scalar, v.Z / scalar);

    }
}

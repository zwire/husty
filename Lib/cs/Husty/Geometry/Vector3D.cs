using static System.Math;

namespace Husty.Geometry;

public enum Axis { X, Y, Z }

public class Vector3D : IEquatable<Vector3D>
{

    // ------ properties ------ //

    public double X { get; }

    public double Y { get; }

    public double Z { get; }

    public double Length { get; }

    public Vector3D UnitVector => Length == 0.0 ? Zero : this / Length;

    public static Vector3D Zero => new(0, 0, 0);


    // ------ constructors ------ //

    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
        Length = Sqrt(X * X + Y * Y + Z * Z);
    }

    public Vector3D(Point3D p1, Point3D p2)
    {
        X = p2.X - p1.X;
        Y = p2.Y - p1.Y;
        Z = p2.Z - p1.Z;
        Length = Sqrt(X * X + Y * Y + Z * Z);
    }


    // ------ public methods ------ //

    public double[] ToArray() => new[] { X, Y, Z };

    public Vector3D Clone() => new(X, Y, Z);

    public Point3D ToPoint3D() => new(X, Y, Z);

    public Angle GetAngle(Axis axis)
    {
        if (axis is Axis.Z)
            return Angle.FromRadian(Atan2(X, Y));
        else if (axis is Axis.Y)
            return Angle.FromRadian(Atan2(Z, X));
        else
            return Angle.FromRadian(Atan2(Y, Z));
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

    public void Deconstruct(out double x, out double y, out double z) => (x, y, z) = (X, Y, Z);

    public bool Equals(Vector3D? obj) => GetHashCode() == obj?.GetHashCode();

    public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();

    public override int GetHashCode() => new { X, Y, Z }.GetHashCode();


    // ------ operators ------ //

    public static Vector3D operator +(Vector3D v1, Vector3D v2) => new(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);

    public static Vector3D operator -(Vector3D v1, Vector3D v2) => new(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);

    public static Vector3D operator -(Vector3D v) => new(-v.X, -v.Y, -v.Z);

    public static Vector3D operator *(Vector3D v, double scalar) => new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static Vector3D operator *(double scalar, Vector3D v) => v * scalar;

    public static Vector3D operator /(Vector3D v, double scalar) => new(v.X / scalar, v.Y / scalar, v.Z / scalar);

    public static bool operator ==(Vector3D a, Vector3D b) => a.Equals(b);

    public static bool operator !=(Vector3D a, Vector3D b) => !a.Equals(b);

}

using System.Text.Json;
using static System.Math;

namespace Husty.Geometry;

public class Point3D : IEquatable<Point3D>
{

    // ------ properties ------ //

    public double X { get; }

    public double Y { get; }

    public double Z { get; }

    public string? ID { get; }

    public static Point3D Zero => new(0, 0, 0);


    // ------ constructors ------ //

    public Point3D(double x, double y, double z, string? id = default)
    {
        X = x; 
        Y = y; 
        Z = z; 
        ID = id;
    }


    // ------ public methods ------ //

    public double DistanceTo(Point3D p) => Sqrt(Pow(p.X - X, 2) + Pow(p.Y - Y, 2) + Pow(p.Z - Z, 2));

    public virtual Point3D Clone() => new(X, Y, Z, ID);

    public Vector3D ToVector3D() => new(X, Y, Z, ID);

    public void Deconstruct(out double x, out double y, out double z, string? id) => (x, y, z, id) = (X, Y, Z, ID);

    public bool Equals(Point3D? obj) => GetHashCode() == obj?.GetHashCode();

    public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();

    public override int GetHashCode() => new { X, Y, Z, ID }.GetHashCode();

    public override string ToString() => JsonSerializer.Serialize(this);


    // ------ operators ------ //

    public static Point2D operator +(Point3D p, Vector3D v) => new(p.X + v.X, p.Y + v.Y, p.ID);

    public static Point2D operator -(Point3D p, Vector3D v) => new(p.X - v.X, p.Y - v.Y, p.ID);

    public static bool operator ==(Point3D a, Point3D b) => a.Equals(b);

    public static bool operator !=(Point3D a, Point3D b) => !a.Equals(b);

}

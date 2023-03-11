using System.Text.Json;
using static System.Math;

namespace Husty.Geometry;

public class Vector2D : IEquatable<Vector2D>
{

    // ------ properties ------ //

    public double X { get; }

    public double Y { get; }

    public string? ID { get; }

    public Angle ClockwiseAngleFromY => Angle.FromRadian(Atan2(X, Y));

    public Angle CounterClockwiseAngleFromX => Angle.FromRadian(Atan2(Y, X));

    public double Length { get; }

    public Vector2D UnitVector => Length == 0.0 ? Zero : this / Length;

    public static Vector2D Zero { get; } = new(0, 0);


    // ------ constructors ------ //

    public Vector2D(double x, double y, string? id = default)
    { 
        X = x;
        Y = y;
        Length = Sqrt(X * X + Y * Y);
        ID = id;
    }

    public Vector2D(Point2D p1, Point2D p2, string? id = default) 
    {
        X = p2.X - p1.X; 
        Y = p2.Y - p1.Y;
        Length = Sqrt(X * X + Y * Y);
        ID = id;
    }

    public Vector2D(Angle direction, double length, string? id = default)
    {
        X = length * Cos(direction.Radian);
        Y = length * Sin(direction.Radian);
        Length = length;
        ID = id;
    }


    // ------ public methods ------ //

    public Angle GetClockwiseAngleFrom(Vector2D v)
    {
        return ClockwiseAngleFromY - v.ClockwiseAngleFromY;
    }

    public Vector2D Rotate(Angle angle)
    {
        var x = X * Cos(angle.Radian) - Y * Sin(angle.Radian);
        var y = X * Sin(angle.Radian) + Y * Cos(angle.Radian);
        return new(x, y, ID);
    }

    public Vector2D Clone() => new(X, Y, ID);

    public Vector3D ToVector3D() => new(X, Y, 0, ID);

    public Point2D ToPoint2D() => new(X, Y, ID);

    public void Deconstruct(out double x, out double y) => (x, y) = (X, Y);

    public void Deconstruct(out double x, out double y, out string? id) => (x, y, id) = (X, Y, ID);

    public bool Equals(Vector2D? obj) => GetHashCode() == obj?.GetHashCode();

    public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();

    public override int GetHashCode() => new { X, Y, ID }.GetHashCode();

    public override string ToString() => JsonSerializer.Serialize(this);


    // ------ operators ------ //

    public static Vector2D operator +(Vector2D v1, Vector2D v2) => new(v1.X + v2.X, v1.Y + v2.Y, v1.ID);

    public static Vector2D operator -(Vector2D v1, Vector2D v2) => new(v1.X - v2.X, v1.Y - v2.Y, v1.ID);

    public static Vector2D operator -(Vector2D v) => new(-v.X, -v.Y, v.ID);

    public static Vector2D operator *(Vector2D v, double scalar) => new(v.X * scalar, v.Y * scalar, v.ID);

    public static Vector2D operator *(double scalar, Vector2D v) => v * scalar;

    public static Vector2D operator /(Vector2D v, double scalar) => new(v.X / scalar, v.Y / scalar, v.ID);

    public static bool operator ==(Vector2D a, Vector2D b) => a.Equals(b);

    public static bool operator !=(Vector2D a, Vector2D b) => !a.Equals(b);

}

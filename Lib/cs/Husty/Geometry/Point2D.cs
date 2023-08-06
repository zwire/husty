using System.Text.Json;
using static System.Math;

namespace Husty.Geometry;

public class Point2D : IEquatable<Point2D>
{

  // ------ properties ------ //

  public double X { get; }

  public double Y { get; }

  public string? ID { get; }

  public static Point2D Zero { get; } = new(0, 0);


  // ------ constructors ------ //

  public Point2D(double x, double y, string? id = default)
  {
    X = x;
    Y = y;
    ID = id;
  }


  // ------ public methods ------ //

  public double DistanceTo(Point2D p) => Sqrt(Pow(p.X - X, 2) + Pow(p.Y - Y, 2));

  public virtual Point2D Clone() => new(X, Y, ID);

  public Point3D ToPoint3D() => new(X, Y, 0, ID);

  public Vector2D ToVector2D() => new(X, Y, ID);

  public void Deconstruct(out double x, out double y) => (x, y) = (X, Y);

  public void Deconstruct(out double x, out double y, out string? id) => (x, y, id) = (X, Y, ID);

  public bool Equals(Point2D? obj) => GetHashCode() == obj?.GetHashCode();

  public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();

  public override int GetHashCode() => new { X, Y, ID }.GetHashCode();

  public override string ToString() => JsonSerializer.Serialize(this);


  // ------ operators ------ //

  public static Point2D operator +(Point2D p, Vector2D v) => new(p.X + v.X, p.Y + v.Y, p.ID);

  public static Point2D operator -(Point2D p, Vector2D v) => new(p.X - v.X, p.Y - v.Y, p.ID);

  public static bool operator ==(Point2D a, Point2D b) => a.Equals(b);

  public static bool operator !=(Point2D a, Point2D b) => !a.Equals(b);

}


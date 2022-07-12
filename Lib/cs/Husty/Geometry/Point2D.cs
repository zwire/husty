using System;
using static System.Math;

namespace Husty.Geometry
{

    public class Point2D : IEquatable<Point2D>
    {

        // ------ properties ------ //

        public double X { get; }

        public double Y { get; }

        public static Point2D Zero => new(0, 0);


        // ------ constructors ------ //

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }


        // ------ public methods ------ //

        public double DistanceTo(Point2D p) => Sqrt(Pow(p.X - X, 2) + Pow(p.Y - Y, 2));

        public double[] ToArray() => new[] { X, Y };

        public Point2D Clone() => new(X, Y);

        public Point3D ToPoint3D() => new(X, Y, 0);

        public Vector2D ToVector2D() => new(X, Y);

        public void Deconstruct(out double x, out double y) => (x, y) = (X, Y);

        public bool Equals(Point2D? obj) => GetHashCode() == obj?.GetHashCode();

        public override bool Equals(object? obj) => GetHashCode() == obj?.GetHashCode();

        public override int GetHashCode() => new { X, Y }.GetHashCode();


        // ------ operators ------ //

        public static Point2D operator +(Point2D p, Vector2D v) => new(p.X + v.X, p.Y + v.Y);

        public static Point2D operator -(Point2D p, Vector2D v) => new(p.X - v.X, p.Y - v.Y);

        public static bool operator ==(Point2D a, Point2D b) => a.Equals(b);

        public static bool operator !=(Point2D a, Point2D b) => !a.Equals(b);

    }

}

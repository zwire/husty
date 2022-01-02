using static System.Math;

namespace Husty
{
    public class Vector2D
    {

        // ------ properties ------ //

        public double X { get; }

        public double Y { get; }

        public Angle ClockwiseAngleFromY => new(Atan2(X, Y), AngleType.Radian);

        public Angle CounterClockwiseAngleFromX => new(Atan2(Y, X), AngleType.Radian);

        public double Length => Sqrt(X * X + Y * Y) + 1e-15;

        public Vector2D UnitVector => this / Length;


        // ------ constructors ------ //

        public Vector2D(double x, double y) { X = x; Y = y; }

        public Vector2D(Point2D p1, Point2D p2) { X = p2.X - p1.X; Y = p2.Y - p1.Y; }


        // ------ public methods ------ //

        public Angle GetClockwiseAngleFrom(Vector2D v)
        {
            return ClockwiseAngleFromY - v.ClockwiseAngleFromY;
        }

        public Vector2D Rotate(Angle angle)
        {
            var x = X * Cos(angle.Radian) - Y * Sin(angle.Radian);
            var y = X * Sin(angle.Radian) + Y * Cos(angle.Radian);
            return new(x, y);
        }

        public double[] ToArray() => new[] { X, Y };

        public Vector3D ToVector3D() => new(X, Y, 0);
        

        // ------ operators ------ //

        public static Vector2D operator +(Vector2D v1, Vector2D v2) => new(v1.X + v2.X, v1.Y + v2.Y);

        public static Vector2D operator -(Vector2D v1, Vector2D v2) => new(v1.X - v2.X, v1.Y - v2.Y);

        public static Vector2D operator -(Vector2D v) => new(-v.X, -v.Y);

        public static Vector2D operator *(Vector2D v, double scalar) => new(v.X * scalar, v.Y * scalar);

        public static Vector2D operator *(double scalar, Vector2D v) => v * scalar;

        public static Vector2D operator /(Vector2D v, double scalar) => new(v.X / scalar, v.Y / scalar);

    }
}

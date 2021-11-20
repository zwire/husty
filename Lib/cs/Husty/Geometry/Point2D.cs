using static System.Math;

namespace Husty
{

    public record Point2D(double X, double Y)
    {

        // ------ public methods ------ //

        public double DistanceTo(Point2D p) => Sqrt(Pow(p.X - X, 2) + Pow(p.Y - Y, 2));

        public double[] ToArray() => new[] { X, Y };


        // ------ operators ------ //

        public static Point2D operator +(Point2D p, Vector2D v) => new(p.X + v.X, p.Y + v.Y);

        public static Point2D operator -(Point2D p, Vector2D v) => new(p.X - v.X, p.Y - v.Y);

    }

}

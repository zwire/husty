using static System.Math;

namespace Husty
{

    public class Point3D
    {

        // ------ properties ------ //

        public double X { get; }

        public double Y { get; }

        public double Z { get; }


        // ------ constructors ------ //

        public Point3D(double x, double y, double z) { X = x; Y = y; Z = z; }


        // ------ public methods ------ //

        public double DistanceTo(Point3D p) => Sqrt(Pow(p.X - X, 2) + Pow(p.Y - Y, 2) + Pow(p.Z - Z, 2));

        public double[] ToArray() => new[] { X, Y, Z };


        // ------ operators ------ //

        public static Point2D operator +(Point3D p, Vector3D v) => new(p.X + v.X, p.Y + v.Y);

        public static Point2D operator -(Point3D p, Vector3D v) => new(p.X - v.X, p.Y - v.Y);

    }
}

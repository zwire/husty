using System;
using OpenCvSharp;
using static System.Math;

namespace Husty.OpenCvSharp
{

    // y = ax + b
    // ax - y + b = 0
    public class Line2D
    {


        // ------ Properties ------ //

        public double Slope { private set; get; }

        public double Intercept { private set; get; }


        // ------ Constructors ------ //

        public Line2D(double slope, double intercept)
        {
            Slope = slope + 1e-7;
            Intercept = intercept + 1e-7;
        }

        public Line2D(HoughCoordinateSystem hough)
        {
            Slope = -1.0 / (Tan(hough.ThetaRadian) + 1e-7);
            Intercept = hough.Rho / (Sin(hough.ThetaRadian) + 1e-7);
        }

        public Line2D(Point2d start, Point2d end)
        {
            if (start.X == end.X && start.Y == end.Y)
                throw new ArgumentException("Require: p1.X != p2.X || p1.Y != p2.Y");
            var dx = end.X - start.X + 1e-7;
            var dy = end.Y - start.Y + 1e-7;
            Slope = dy / dx;
            Intercept = start.Y - start.X * Slope;
        }

        public Line2D(Point2d p, double slope)
        {
            Slope = slope;
            Intercept = p.Y - p.X * Slope;
        }


        // ------ Methods ------ //

        public virtual double GetY(double x)
        {
            return Slope * x + Intercept;
        }

        public virtual double GetX(double y)
        {
            return (y - Intercept) / Slope;
        }

        public double DistanceTo(Point2d p)
        {
            return Abs(Slope * p.X - p.Y + Intercept) / Sqrt(Slope * Slope + 1);
        }

        public Line2D GetPerpendicularLine(Point2d p)
        {
            var slope = -1.0 / Slope;
            var intercept = p.Y - slope * p.X;
            return new(slope, intercept);
        }

        public Point2d GetPerpendicularFoot(Point2d p)
        {
            var line = GetPerpendicularLine(p);
            var c = line.Slope;
            var d = line.Intercept;
            var x = (d - Intercept) / (Slope - c);
            var y = c * x + d;
            return new(x, y);
        }

        public Line2D ChangeXY()
        {
            return new(1.0 / Slope, -Intercept / Slope);
        }

    }
}

using System;
using OpenCvSharp;

namespace Husty.OpenCvSharp
{
    public class Arc2D : IClosedCurve
    {

        // ------ Properties ------ //

        public double Radius { private set; get; }

        public double Curvature { private set; get; }

        public double AngleRadian { private set; get; }

        public Point2d Center { private set; get; }

        public Point2d Start { private set; get; }

        public Point2d End { private set; get; }


        // ------ Constructors ------ //

        public Arc2D(Point2d start, Point2d center, double angleRadian, bool clockwise)
        {
            AngleRadian = clockwise ? -angleRadian : angleRadian;
            Center = center;
            Radius = center.DistanceTo(start);
            Curvature = 1.0 / Radius;
            Start = start;
            End = center + (start - center).Rotate(AngleRadian);
        }


        // ------ Methods ------ //

        public Point2d[] GetEquallySpacedPoints(double interval)
        {
            var radInterval = CalcAngleRadian(interval);
            var count = (int)(Math.Abs(AngleRadian) / radInterval);
            var points = new Point2d[count];
            var vec = Start - Center;
            for (int i = 0; i < count; i++)
                points[i] = Center + vec.Rotate(radInterval * i);
            return points;
        }

        private double CalcAngleRadian(double arcLength) => arcLength / Radius;

        private double CalcArcLength(double angleRadian) => angleRadian * Radius;

    }
}

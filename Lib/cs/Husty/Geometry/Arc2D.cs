using System.Collections.Generic;

namespace Husty.Geometry
{
    public class Arc2D
    {

        // ------ properties ------ //

        public double Radius { get; }

        public double Curvature { get; }

        public Angle Angle { get; }

        public Point2D Center { get; }

        public Point2D Start { get; }

        public Point2D End { get; }


        // ------ constructors ------ //

        public Arc2D(Point2D start, Point2D center, Angle counterClockwiseAngle)
        {
            Angle = counterClockwiseAngle;
            Center = center;
            Radius = center.DistanceTo(start);
            Curvature = 1.0 / Radius;
            Start = start;
            End = center + new Vector2D(center, start).Rotate(Angle);
        }


        // ------ public methods ------ //

        public Arc2D GetReverse() => new(End, Center, -Angle);

        public Point2D[] ApproximateAsPoints(Angle interval)
        {
            var points = new List<Point2D>();
            var initVec = new Vector2D(Center, Start);
            if (Angle.Radian > 0)
                for (double angle = 0; angle < Angle.Radian; angle += interval.Radian)
                    points.Add(Center + initVec.Rotate(Angle.FromRadian(angle)));
            else
                for (double angle = 0; angle > Angle.Radian; angle -= interval.Radian)
                    points.Add(Center + initVec.Rotate(Angle.FromRadian(angle)));
            points.Add(End);
            return points.ToArray();
        }


        // ------ private methods ------ //

        private double CalcAngleRadian(double arcLength) => arcLength / Radius;

        private double CalcArcLength(Angle angle) => angle.Radian * Radius;

    }
}

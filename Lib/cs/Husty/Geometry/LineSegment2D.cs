using System;
using System.Collections.Generic;
using static System.Math;

namespace Husty
{
    public class LineSegment2D : Line2D
    {

        // ------ properties ------ //

        public Point2D Start { private set; get; }

        public Point2D End { private set; get; }

        public double Length => Sqrt(Pow(End.X - Start.X, 2) + Pow(End.Y - Start.Y, 2));


        // ------ constructors ------ //

        public LineSegment2D(Point2D start, Point2D end) : base(start, end)
        {
            Start = start;
            End = end;
        }


        // ------ public methods ------ //

        public Vector2D ToVector2D() => new(End.X - Start.X, End.Y - Start.Y);

        public override double GetY(double x)
        {
            if (Start.X < End.X)
                if (x >= Start.X && x <= End.X)
                    return base.GetY(x);
                else
                if (x >= End.X && x <= Start.X)
                    return base.GetY(x);
            throw new ArgumentException("Given X is out of segment range.");
        }

        public override double GetX(double y)
        {
            if (Start.Y < End.Y)
                if (y >= Start.Y && y <= End.Y)
                    return base.GetX(y);
                else
                if (y >= End.Y && y <= Start.Y)
                    return base.GetX(y);
            throw new ArgumentException("Given Y is out of segment range.");
        }

        public Point2D GetNearestPoint(Point2D point)
        {
            var perf = GetPerpendicularFoot(point);
            if (perf.X < Start.X && perf.X < End.X)
            {
                if (Start.X < End.X)
                    return Start;
                else
                    return End;
            }
            else if (perf.X > Start.X && perf.X > End.X)
            {
                if (Start.X > End.X)
                    return Start;
                else
                    return End;
            }
            return perf;
        }

        public Point2D[] ApproximateAsPoints(double intervalMeter)
        {
            var points = new List<Point2D>();
            var vec = new Vector2D(Start, End).UnitVector;
            for (double l = 0; l < Length; l += intervalMeter)
                points.Add(Start + vec * l);
            return points.ToArray();
        }

    }
}

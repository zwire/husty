using System;
using OpenCvSharp;

namespace Husty.OpenCvSharp
{
    public class LineSegment2D : Line2D, IClosedCurve
    {

        // ------ Properties ------ //

        public Point2d Start { private set; get; }

        public Point2d End { private set; get; }


        // ------ Constructors ------ //

        public LineSegment2D(Point2d start, Point2d end) : base(start, end)
        {
            Start = start;
            End = end;
        }


        // ------ Methods ------ //

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

        public Point2d[] GetEquallySpacedPoints(double interval)
        {
            var count = (int)(Start.DistanceTo(End) / interval);
            var step = (Vector.CreateVec2d(Start, End).Normalize() * interval).ToPoint2d();
            var points = new Point2d[count];
            for (int i = 0; i < count; i++)
                points[i] = Start + step * i;
            return points;
        }

        public Point2d GetNearestPoint(Point2d point)
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

    }
}

using System;

namespace Husty.Geometry
{
    public class BezierCurve : BSplineCurve
    {

        public BezierCurve(Point2D[] points, int count) : base(points, points.Length - 1, count) { }


        // https://watlab-blog.com/2022/01/03/bezier-curve/
        public static TrajectoryPoint GenerateCurvePoint(Point2D[] points, double t)
        {
            var n = points.Length - 1;
            var px = 0.0;
            var py = 0.0;
            var dx = 0.0;
            var dy = 0.0;
            for (int i = 0; i < n + 1; i++)
            {
                var nCk = n.Factorial() / (i.Factorial() * (n - i).Factorial());
                var b = nCk * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
                var db = nCk * (i * Math.Pow(t, i - 1) * Math.Pow(1 - t, n - i) - (n - i) * Math.Pow(t, i) * Math.Pow(1 - t, n - i - 1));
                px += b * points[i].X;
                py += b * points[i].Y;
                dx += db * points[i].X;
                dy += db * points[i].Y;
            }
            return new(new(px, py), new Vector2D(dx, dy).ClockwiseAngleFromY);
        }

    }
}

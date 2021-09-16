using OpenCvSharp;

namespace Husty.OpenCvSharp
{

    public static class PointExtensions
    {

        public static Point ToPoint(this Point2f point)
        {
            return (Point)point;
        }

        public static Point ToPoint(this Point2d point)
        {
            return (Point)point;
        }

        public static Point2f ToPoint2f(this Point point)
        {
            return point;
        }

        public static Point2f ToPoint2f(this Point2d point)
        {
            return new((float)point.X, (float)point.Y);
        }

        public static Point2d ToPoint2d(this Point point)
        {
            return point;
        }

        public static Point2d ToPoint2d(this Point2f point)
        {
            return new(point.X, point.Y);
        }

    }

}

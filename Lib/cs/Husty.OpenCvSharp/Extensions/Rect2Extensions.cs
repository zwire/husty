using OpenCvSharp;

namespace Husty.OpenCvSharp
{

    public static class Rect2Extensions
    {

        public static Rect ToRect(this Rect2f rect)
        {
            return new(new(rect.Left, rect.Top), new(rect.Width, rect.Height));
        }

        public static Rect ToRect(this Rect2d rect)
        {
            return new(new(rect.Left, rect.Top), new(rect.Width, rect.Height));
        }

        public static Rect2f ToRect2f(this Rect rect)
        {
            return new(new(rect.Left, rect.Top), new(rect.Width, rect.Height));
        }

        public static Rect2f ToRect2f(this Rect2d rect)
        {
            return new(new((float)rect.Left, (float)rect.Top), new(rect.Width, rect.Height));
        }

        public static Rect2d ToRect2d(this Rect rect)
        {
            return new(new(rect.Left, rect.Top), new(rect.Width, rect.Height));
        }

        public static Rect2d ToRect2d(this Rect2f rect)
        {
            return new(new(rect.Left, rect.Top), new(rect.Width, rect.Height));
        }

        public static Rect Scale(this Rect rect, double x, double y)
        {
            return new(new(rect.Left * x, rect.Top * y), new(rect.Width * x, rect.Height * y));
        }

        public static Rect2f Scale(this Rect2f rect, double x, double y)
        {
            return new(new((float)(rect.Left * x), (float)(rect.Top * y)), new(rect.Width * x, rect.Height * y));
        }

        public static Rect2d Scale(this Rect2d rect, double x, double y)
        {
            return new(new(rect.Left * x, rect.Top * y), new(rect.Width * x, rect.Height * y));
        }

        public static Point GetCenter(this Rect rect)
        {
            return new(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
        }

        public static Point2f GetCenter(this Rect2f rect)
        {
            return new(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
        }

        public static Point2d GetCenter(this Rect2d rect)
        {
            return new(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
        }

        public static Point[] ToPointArray(this Rect rect)
        {
            var points = new Point[4];
            points[0] = new(rect.Left, rect.Top);
            points[1] = new(rect.Left, rect.Bottom);
            points[2] = new(rect.Right, rect.Bottom);
            points[3] = new(rect.Right, rect.Top);
            return points;
        }

        public static Point2f[] ToPointArray(this Rect2f rect)
        {
            var points = new Point2f[4];
            points[0] = new(rect.Left, rect.Top);
            points[1] = new(rect.Left, rect.Bottom);
            points[2] = new(rect.Right, rect.Bottom);
            points[3] = new(rect.Right, rect.Top);
            return points;
        }

        public static Point2d[] ToPointArray(this Rect2d rect)
        {
            var points = new Point2d[4];
            points[0] = new(rect.Left, rect.Top);
            points[1] = new(rect.Left, rect.Bottom);
            points[2] = new(rect.Right, rect.Bottom);
            points[3] = new(rect.Right, rect.Top);
            return points;
        }

    }

}

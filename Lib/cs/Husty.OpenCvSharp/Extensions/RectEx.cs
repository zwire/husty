using OpenCvSharp;

namespace Husty.OpenCvSharp.Extensions;

public static class RectEx
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

    public static double GetAspectRatio(this Rect rect, bool belowOne = false)
    {
        return belowOne
            ? Math.Min((float)rect.Width / rect.Height, (float)rect.Height / rect.Width)
            : (float)rect.Width / rect.Height;
    }

    public static double GetAspectRatio(this Rect2f rect, bool belowOne = false)
    {
        return belowOne
            ? Math.Min(rect.Width / rect.Height, rect.Height / rect.Width)
            : rect.Width / rect.Height;
    }

    public static double GetAspectRatio(this Rect2d rect, bool belowOne = false)
    {
        return belowOne
            ? Math.Min(rect.Width / rect.Height, rect.Height / rect.Width)
            : rect.Width / rect.Height;
    }

    public static int Area(this Rect rect)
    {
        return rect.Width * rect.Height;
    }

    public static float Area(this Rect2f rect)
    {
        return rect.Width * rect.Height;
    }

    public static double Area(this Rect2d rect)
    {
        return rect.Width * rect.Height;
    }

    public static Rect? GetCommonPart(this Rect rect1, Rect rect2)
    {
        return rect1.ToRect2d().GetCommonPart(rect2.ToRect2d())?.ToRect();
    }

    public static Rect2f? GetCommonPart(this Rect2f rect1, Rect2f rect2)
    {
        return rect1.ToRect2d().GetCommonPart(rect2.ToRect2d())?.ToRect2f();
    }

    public static Rect2d? GetCommonPart(this Rect2d rect1, Rect2d rect2)
    {
        if (rect1.Right < rect2.Left || rect1.Left > rect2.Right || rect1.Bottom < rect2.Top || rect1.Top > rect2.Bottom) return null;
        var left = Math.Max(rect1.Left, rect2.Left);
        var right = Math.Min(rect1.Right, rect2.Right);
        var top = Math.Max(rect1.Top, rect2.Top);
        var bottom = Math.Min(rect1.Bottom, rect2.Bottom);
        return new(left, top, right - left, bottom - top);
    }

    public static double IoU(this Rect rect1, Rect rect2)
    {
        return rect1.ToRect2d().IoU(rect2.ToRect2d());
    }

    public static double IoU(this Rect2f rect1, Rect2f rect2)
    {
        return rect1.ToRect2d().IoU(rect2.ToRect2d());
    }

    public static double IoU(this Rect2d rect1, Rect2d rect2)
    {
        var common = rect1.GetCommonPart(rect2);
        if (common is Rect2d c)
        {
            var union = rect1.Width * rect1.Height + rect2.Width * rect2.Height - c.Area();
            return c.Area() / union;
        }
        return 0;
    }

    public static Point TopRight(this Rect r) => new(r.Right, r.Top);

    public static Point2f TopRight(this Rect2f r) => new(r.Right, r.Top);

    public static Point2d TopRight(this Rect2d r) => new(r.Right, r.Top);

    public static Point BottomLeft(this Rect r) => new(r.Left, r.Bottom);

    public static Point2f BottomLeft(this Rect2f r) => new(r.Left, r.Bottom);

    public static Point2d BottomLeft(this Rect2d r) => new(r.Left, r.Bottom);

    public static (int Left, int Top, int Right, int Bottom) Deconstruct(this Rect r) => (r.X, r.Y, r.Right, r.Bottom);
    
    public static (float Left, float Top, float Right, float Bottom) Deconstruct(this Rect2f r) => (r.X, r.Y, r.Right, r.Bottom);
    
    public static (double Left, double Top, double Right, double Bottom) Deconstruct(this Rect2d r) => (r.X, r.Y, r.Right, r.Bottom);

}


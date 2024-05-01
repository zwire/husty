using OpenCvSharp;
using static System.Math;

namespace Husty.OpenCvSharp.Extensions;

public static class PointEx
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

  public static Point Rotate(this Point point, double radian)
  {
    var x0 = point.X;
    var y0 = point.Y;
    var x = Cos(radian) * x0 - Sin(radian) * y0;
    var y = Sin(radian) * x0 + Cos(radian) * y0;
    return new(x, y);
  }

  public static Point2f Rotate(this Point2f point, double radian)
  {
    var x0 = point.X;
    var y0 = point.Y;
    var x = (float)(Cos(radian) * x0 - Sin(radian) * y0);
    var y = (float)(Sin(radian) * x0 + Cos(radian) * y0);
    return new(x, y);
  }

  public static Point2d Rotate(this Point2d point, double radian)
  {
    var x0 = point.X;
    var y0 = point.Y;
    var x = Cos(radian) * x0 - Sin(radian) * y0;
    var y = Sin(radian) * x0 + Cos(radian) * y0;
    return new(x, y);
  }

  public static Point Scale(this Point point, double x, double y)
  {
    return new(point.X * x, point.Y * y);
  }

  public static Point2f Scale(this Point2f point, double x, double y)
  {
    return new((float)(point.X * x), (float)(point.Y * y));
  }

  public static Point2d Scale(this Point2d point, double x, double y)
  {
    return new(point.X * x, point.Y * y);
  }

}


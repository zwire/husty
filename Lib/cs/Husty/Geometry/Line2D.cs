using static System.Math;

namespace Husty.Geometry;

// y = ax + b
// ax - y + b = 0
public class Line2D
{

  // ------ properties ------ //

  public double Slope { private set; get; }

  public double Intercept { private set; get; }


  // ------ constructors ------ //

  public Line2D(double slope, double intercept)
  {
    Slope = slope + 1e-15;
    Intercept = intercept + 1e-15;
  }

  public Line2D(Point2D start, Point2D end)
  {
    if (start.X == end.X && start.Y == end.Y)
      throw new ArgumentException("Require: p1.X != p2.X || p1.Y != p2.Y");
    var dx = end.X - start.X + 1e-15;
    var dy = end.Y - start.Y + 1e-15;
    Slope = dy / dx;
    Intercept = start.Y - start.X * Slope;
  }

  public Line2D(Point2D p, double slope)
  {
    Slope = slope;
    Intercept = p.Y - p.X * Slope;
  }

  public Line2D(HoughCoordinateSystem hough)
  {
    Slope = -1.0 / (Tan(hough.Theta.Radian) + 1e-15);
    Intercept = hough.Rho / (Sin(hough.Theta.Radian) + 1e-15);
  }

  // ------ public methods ------ //

  public virtual double GetY(double x) => Slope * x + Intercept;

  public virtual double GetX(double y) => (y - Intercept) / Slope;

  public Line2D ChangeXY() => new(1.0 / Slope, -Intercept / Slope);

  public double DistanceTo(Point2D p) => Abs(Slope * p.X - p.Y + Intercept) / Sqrt(Slope * Slope + 1);

  public Line2D GetPerpendicularLine(Point2D p)
  {
    var slope = -1.0 / Slope;
    var intercept = p.Y - slope * p.X;
    return new(slope, intercept);
  }

  public Point2D GetPerpendicularFoot(Point2D p)
  {
    var line = GetPerpendicularLine(p);
    var c = line.Slope;
    var d = line.Intercept;
    var x = (d - Intercept) / (Slope - c);
    var y = c * x + d;
    return new(x, y);
  }

  public Point2D GetIntersection(Line2D line)
  {
    var x = (line.Intercept - Intercept) / (Slope - line.Slope);
    var y = (Slope * line.Intercept - Intercept * line.Slope) / (Slope - line.Slope);
    return new(x, y);
  }

}

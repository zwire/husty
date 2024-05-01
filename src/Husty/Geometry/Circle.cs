namespace Husty.Geometry;

public class Circle
{

  // ------ properties ------ //

  public Point2D Center { get; }

  public double Radius { get; }


  // ------ constructors ------ //

  public Circle(Point2D center, double radius)
  {
    Center = center;
    Radius = radius;
  }


  // ------ public methods ------ //

  public Point2D[] ApproximateAsPointsOnCircle(Angle interval)
  {
    var vecs = GetOnCircularUniformDistribution(Radius, interval);
    return vecs.Select(v => Center + v).ToArray();
  }

  public Point2D[] ApproximateAsPointsInCircle(int count)
  {
    var vecs = GetInsideCircularUniformDistribution(Radius, count);
    return vecs.Select(v => Center + v).ToArray();
  }

  public static Vector2D[] GetOnCircularUniformDistribution(double radius, Angle interval)
  {
    var vecs = new List<Vector2D>();
    for (double angle = -Math.PI; angle < Math.PI; angle += interval.Radian)
    {
      var x = radius * Math.Cos(angle);
      var y = radius * Math.Sin(angle);
      vecs.Add(new(x, y));
    }
    return vecs.ToArray();
  }

  public static Vector2D[] GetInsideCircularUniformDistribution(double radius, int count)
  {
    var rand = new Random();
    var pts = new Vector2D[count];
    for (int i = 0; i < count; i++)
    {
      var r = radius * Math.Sqrt(rand.NextDouble());
      var theta = 2 * Math.PI * rand.NextDouble();
      var x = r * Math.Cos(theta);
      var y = r * Math.Sin(theta);
      pts[i] = new(x, y);
    }
    return pts;
  }

}

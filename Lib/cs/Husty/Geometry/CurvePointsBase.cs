namespace Husty.Geometry;

public abstract class CurvePointsBase
{

    // ------ fields ------ //

    private readonly Point2D[] _curvePoints;


    // ------ properties ------ //

    public int Count { get; }


    // ------ constructor ------ //

    public CurvePointsBase(Point2D[] curvePoints, int count)
    {
        _curvePoints = curvePoints;
        Count = count;
    }


    // ------ public methods ------ //

    public Point2D[] GetPoints() => _curvePoints;

    public Pose2D[] GetTrajectoryPoints()
    {
        var results = new Pose2D[_curvePoints.Length];
        for (int i = 0; i < results.Length; i++)
            results[i] = new(_curvePoints[i], GetHeading(i));
        return results;
    }

    public Angle GetHeading(int index)
    {
        if (index < 0 || index > _curvePoints.Length - 1)
            throw new IndexOutOfRangeException(nameof(index));
        if (index == _curvePoints.Length - 1) index--;
        var p0 = _curvePoints[index];
        var p1 = _curvePoints[index + 1];
        return new Vector2D(p0, p1).ClockwiseAngleFromY;
    }

    public double GetCurvature(int index)
    {
        if (index < 0 || index > _curvePoints.Length - 1)
            throw new IndexOutOfRangeException(nameof(index));
        if (index is 0) index++;
        if (index == _curvePoints.Length - 1) index--;
        var p0 = _curvePoints[index - 1];
        var p1 = _curvePoints[index];
        var p2 = _curvePoints[index + 1];
        if (new Vector2D(p0, p1).GetClockwiseAngleFrom(new Vector2D(p0, p2)).Radian is 0) return 0;
        // 幾何学的に曲率を求める
        // https://blog.mori-soft.com/entry/%3Fp%3D170
        var m0 = new Point2D((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2);
        var tana = Math.Tan((new Vector2D(p0, p1).CounterClockwiseAngleFromX + Angle.FromDegree(90)).Radian);
        var m1 = new Point2D((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        var tanb = Math.Tan((new Vector2D(p1, p2).CounterClockwiseAngleFromX + Angle.FromDegree(90)).Radian);
        var cx = (m0.Y - m1.Y - m0.X * tana + m1.X * tanb) / (tanb - tana);
        var cy = m0.Y - (m0.X - cx) * tana;
        return 1.0 / Math.Sqrt(Math.Pow(m0.X - cx, 2) + Math.Pow(m0.Y - cy, 2));
    }

    public double GetMaxCurvature()
    {
        var max = 0.0;
        for (int i = 0; i < Count; i++)
        {
            var curvature = GetCurvature(i);
            if (curvature > max) max = curvature;
        }
        return max;
    }
}

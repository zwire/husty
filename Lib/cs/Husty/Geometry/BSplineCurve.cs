using System;
using System.Linq;

namespace Husty.Geometry
{

    public record TrajectoryPoint(Point2D Position, Angle Heading);

    // https://tajimarobotics.com/basis-spline-interpolation-program/
    public class BSplineCurve
    {

        // ------ fields ------ //

        private readonly Point2D[] _curvePoints;


        // ------ properties ------ //

        public int Count { get; }


        // ------ constructor ------ //

        public BSplineCurve(Point2D[] points, int degree, int count)
        {
            var knotCount = points.Length + degree + 1;
            var knotVector = GetOpenUniformKnotVector(knotCount, degree);
            _curvePoints = Enumerable.Repeat(Point2D.Zero, count).ToArray();
            _curvePoints[0] = points[0];
            for (int j = 0; j < points.Length; j++)
            {
                var func = GetBasisFunction(knotVector, j, degree);
                var vec = points[j].ToVector2D();
                for (int i = 1; i < _curvePoints.Length; i++)
                {
                    _curvePoints[i] += vec * func(i / (double)count);
                }
            }
            Count = count;
        }


        // ------ public methods ------ //

        public Point2D[] GetPoints() => _curvePoints;

        public TrajectoryPoint[] GetTrajectoryPoints()
        {
            var results = new TrajectoryPoint[_curvePoints.Length];
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


        // ------ private methods ------ //

        private static double[] GetOpenUniformKnotVector(int knotCount, int degree)
        {
            var knotVector = new double[knotCount];
            var max = knotCount - 1 - 2 * degree;
            for (int i = 0; i < knotCount; i++)
            {
                if (i < degree + 1)
                    knotVector[i] = 0;
                else if (i >= knotCount - (degree + 1))
                    knotVector[i] = 1;
                else
                    knotVector[i] = (i - degree) / (double)max;
            }
            return knotVector;
        }

        /// <summary>再帰的に基底関数の出力を計算する</summary>
        /// <param name="knotVector">ノットベクトル</param>
        /// <param name="j">制御点インデックス</param>
        /// <param name="k">基底関数インデックス</param>
        /// <returns></returns>
        private static Func<double, double> GetBasisFunction(double[] knotVector, int j, int k)
        {
            return t =>
            {
                if (k is 0)
                {
                    if (knotVector[j] < t && t <= knotVector[j + 1])
                        return 1.0;
                    else
                        return 0.0;
                }
                else
                {
                    var result = 0.0;
                    if (knotVector[j + k + 1] - knotVector[j + 1] is not 0)
                        result += GetBasisFunction(knotVector, j + 1, k - 1)(t) * (knotVector[j + k + 1] - t) / (knotVector[j + k + 1] - knotVector[j + 1]);
                    if (knotVector[j + k] - knotVector[j] is not 0)
                        result += GetBasisFunction(knotVector, j, k - 1)(t) * (t - knotVector[j]) / (knotVector[j + k] - knotVector[j]);
                    return result;
                }
            };
        }

    }
}

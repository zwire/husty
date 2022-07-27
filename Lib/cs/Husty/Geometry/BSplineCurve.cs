using System;
using System.Linq;

namespace Husty.Geometry
{

    // https://tajimarobotics.com/basis-spline-interpolation-program/
    public class BSplineCurve : CurvePointsBase
    {

        // ------ constructor ------ //

        public BSplineCurve(Point2D[] points, int degree, int count) : base(Init(points, degree, count), count) { }


        // ------ private methods ------ //

        private static Point2D[] Init(Point2D[] points, int degree, int count)
        {
            var knotCount = points.Length + degree + 1;
            var knotVector = GetOpenUniformKnotVector(knotCount, degree);
            var curvePoints = Enumerable.Repeat(Point2D.Zero, count).ToArray();
            curvePoints[0] = points[0];
            for (int j = 0; j < points.Length; j++)
            {
                var func = GetBasisFunction(knotVector, j, degree);
                var vec = points[j].ToVector2D();
                for (int i = 1; i < curvePoints.Length; i++)
                    curvePoints[i] += vec * func(i / (double)count);
            }
            return curvePoints;
        }

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

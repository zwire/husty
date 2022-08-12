using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.Geometry;

public class SplineCurve : CurvePointsBase
{

    // ------ constructors ------ //

    public SplineCurve(Point2D[] points, int count) : base(Init(points, count), count) { }


    // ------ private methods ------ //

    private static Point2D[] Init(Point2D[] points, int count)
    {
        var n = points.Length - 1;
        var dists = new double[n];
        for (int i = 0; i < n; i++)
            dists[i] = points[i].DistanceTo(points[i + 1]);
        var funcX = GetSplineFunction(dists, points.Select(p => p.X).ToArray());
        var funcY = GetSplineFunction(dists, points.Select(p => p.Y).ToArray());
        var length = dists.Sum();
        var interval = length / count;
        var curvePoints = new Point2D[count];
        var index = 0;
        for (double s = 0; s < length; s += interval)
            if (index < curvePoints.Length)
                curvePoints[index++] = new Point2D(funcX(s), funcY(s));
        return curvePoints;

        Func<double, double> GetSplineFunction(double[] h, double[] a)
        {
            var A = DenseMatrix.CreateIdentity(n + 1);
            for (int i = 1; i < n; i++)
            {
                A[i, i] = 2 * (h[i - 1] + h[i]);
                A[i, i - 1] = h[i - 1];
                A[i, i + 1] = h[i];
            }
            var B = DenseVector.Create(n + 1, 0);
            for (int i = 1; i < n; i++)
            {
                B[i] = 3 * (a[i + 1] - a[i]) / h[i] - 3 * (a[i] - a[i - 1]) / h[i - 1];
            }
            var c = (A.Inverse() * B).ToArray();

            var b = new double[n];
            var d = new double[n];
            for (int i = 0; i < n; i++)
            {
                d[i] = (c[i + 1] - c[i]) / 3 / h[i];
                b[i] = (a[i + 1] - a[i]) / h[i] - h[i] * (c[i + 1] + 2 * c[i]) / 3;
            }

            return s =>
            {
                var j = 0;
                var sum = 0.0;
                for (int i = 0; i < n; i++)
                {
                    sum += h[i];
                    if (s > sum) j++;
                    else break;
                }
                var dt = j is 0 ? s : s - sum + h[j];
                return a[j] + (b[j] + (c[j] + d[j] * dt) * dt) * dt;
            };
        }
    }

}

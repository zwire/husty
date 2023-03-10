using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.Filters;

public sealed class UnscentedKalmanFilter : NonlinearStateFilterBase
{

    // ------ fields ------ //

    private readonly double _lambda;
    private readonly double _wi;
    private readonly double _ws0;
    private readonly double _wc0;
    private readonly Vector<double>[] _sigmas;


    // ------ constructors ------ //

    public UnscentedKalmanFilter(
        IEnumerable<double> x0, 
        int observationVectorSize, 
        int controlVectorSize,
        double alpha = 1e-3
    ) : base(x0, observationVectorSize, controlVectorSize)
    {
        // determine weights
        var l = (alpha * alpha - 1) * _k;
        _lambda = Math.Sqrt(_k + l);
        _wi = 1 / (2 * (_k + l));
        _ws0 = l / (l + _k);
        _wc0 = l / (l + _k) + 3 - alpha * alpha;
        _sigmas = new Vector<double>[2 * _k + 1];
    }


    // ------ public methods ------ //

    public override double[] Predict(params double[] u)
    {
        if (Dt <= 0) throw new Exception("Require: Dt > 0");
        UpdateSigmas();
        // move sigma points by the transition function
        // and estimate x depending on the weight
        var uv = DenseVector.OfArray(u);
        var x = Vector<double>.Build.Dense(_k);
        for (int i = 0; i < _sigmas.Length; i++)
        {
            _sigmas[i] = NonlinearTransitionFunction(new(_sigmas[i], uv, Dt));
            x += (i is 0 ? _ws0 : _wi) * _sigmas[i];
        }
        // evaluate prediction errors covariance
        var p = Matrix<double>.Build.Dense(_k, _k);
        for (int i = 0; i < _sigmas.Length; i++)
        {
            var e = DenseMatrix.OfColumnVectors(_sigmas[i] - x);
            p += (i is 0 ? _wc0 : _wi) * e * e.Transpose();
        }
        // update X, P
        X = x;
        P = p + Q;
        return X.ToArray();
    }

    public override double[] Update(params double[] y)
    {
        var yv = DenseVector.OfArray(y);
        UpdateSigmas();
        // estimate sigma points by the observation function
        // and estimate y depending on the weight
        var gammas = new Vector<double>[_sigmas.Length];
        var ym = Vector<double>.Build.Dense(_m);
        for (int i = 0; i < gammas.Length; i++)
        {
            gammas[i] = NonlinearObservationFunction(new(_sigmas[i]));
            ym += (i is 0 ? _ws0 : _wi) * gammas[i];
        }
        // evaluate estimation errors covariance
        var S = R;
        var Cxy = Matrix<double>.Build.Dense(_k, _m);
        for (int i = 0; i < gammas.Length; i++)
        {
            var ey = DenseMatrix.OfColumnVectors(gammas[i] - ym);
            var ex = DenseMatrix.OfColumnVectors(_sigmas[i] - X);
            S += (i is 0 ? _wc0 : _wi) * ey * ey.Transpose();
            Cxy += (i is 0 ? _wc0 : _wi) * ex * ey.Transpose();
        }
        // calculate K and update X, P
        var E = yv - ym;              // innovation
        var K = Cxy * S.Inverse();    // optimal kalman gain
        X += K * E;                   // update state estimate
        P -= K * S * K.Transpose();   // update estimate covariance
        return X.ToArray();
    }


    // ------ private methods ------ //

    /// <summary>put sigma points around the current estimate depending on the weight and P</summary>
    private void UpdateSigmas()
    {
        for (int i = 0; i < P.RowCount; i++)
            if (P[i, i] is 0) P[i, i] += 1e-9;
        var sigmas = _lambda * P.Cholesky().Factor;
        _sigmas[0] = X;
        for (int i = 1; i <= _k; i++)
        {
            _sigmas[i] = X + sigmas.Column(i - 1);
            _sigmas[_k + i] = X - sigmas.Column(i - 1);
        }
    }

}

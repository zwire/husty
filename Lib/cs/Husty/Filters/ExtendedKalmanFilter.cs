using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.Filters;

public sealed class ExtendedKalmanFilter : SequentialBayesianFilterBase
{

    // ------ constructors ------ //

    public ExtendedKalmanFilter(
        IEnumerable<double> x0, 
        int observationVectorSize, 
        int controlVectorSize
    ) : base(x0, observationVectorSize, controlVectorSize) { }


    // ------ public methods ------ //

    /// <remark>please update Jacobian (A: transition) in advance if you need.</remark>
    public override double[] Predict(params double[] u)
    {
        if (Dt <= 0) throw new Exception("Require: Dt > 0");
        X = TransitionFunc(new(X, DenseVector.OfArray(u), Dt));    // predict state estimate
        P = A * P * A.Transpose() + Q;                             // predict estimate covariance
        P = (P + P.Transpose()) * 0.5;
        return X.ToArray();
    }

    /// <remark>please update jacobian (C: observation) in advance if you need.</remark>
    public override double[] Update(params double[] y)
    {
        var E = DenseVector.OfArray(y) - ObservationFunc(new(X));  // innovation
        var S = C * P * C.Transpose() + R;                         // innovation covariance
        var K = P * C.Transpose() * S.Inverse();                   // optimal kalman gain
        X += K * E;                                                // update state estimate
        P -= K * C * P;                                            // update estimate covariance
        P = (P + P.Transpose()) * 0.5;
        return X.ToArray();
    }

}

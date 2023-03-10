using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.Filters;

public sealed class ExtendedKalmanFilter : NonlinearStateFilterBase
{

    // ------ constructors ------ //

    public ExtendedKalmanFilter(
        IEnumerable<double> x0, 
        int observationVectorSize, 
        int controlVectorSize
    ) : base(x0, observationVectorSize, controlVectorSize) { }


    // ------ public methods ------ //

    /// <remark>please update jacobian (A: transition, B: control) in advance if you need.</remark>
    public override double[] Predict(params double[] u)
    {
        if (Dt <= 0) throw new Exception("Require: Dt > 0");
        P = A * P * A.Transpose() + Q;                                          // predict estimate covariance
        X = NonlinearTransitionFunction(new(X, DenseVector.OfArray(u), Dt));    // predict state estimate
        return X.ToArray();
    }

    /// <remark>please update jacobian (C: observation) in advance if you need.</remark>
    public override double[] Update(params double[] y)
    {
        var E = DenseVector.OfArray(y) - NonlinearObservationFunction(new(X));  // innovation
        var S = C * P * C.Transpose() + R;                                      // innovation covariance
        var K = P * C.Transpose() * S.Inverse();                                // optimal kalman gain
        X += K * E;                                                             // update state estimate
        P -= K * C * P;                                                         // update estimate covariance
        return X.ToArray();
    }

}

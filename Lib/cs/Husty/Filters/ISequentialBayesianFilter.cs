using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.Filters;

/// <remarks>
/// -- linear state expression --
/// Xt+1 = (AXt + BUt) * dt + W
/// Yt+1 = CXt+1 + V
/// -- non-linear state expression --
/// Xt+1 = nonlinear_transition_function(Xt, Ut, W, dt)
/// Yt+1 = nonlinear_observation_function(Xt+1, V)
/// where: W ~ Q, V ~ R
/// </remarks>

public interface ISequentialBayesianFilter
{

    /// <summary>time span</summary>
    public double Dt { set; get; }

    /// <summary>the state-transition model</summary>
    public Matrix<double> A { set; get; }

    /// <summary>the control-input model</summary>
    public Matrix<double> B { set; get; }

    /// <summary>the observation model</summary>
    public Matrix<double> C { set; get; }

    /// <summary>the covariance of the process noise</summary>
    public Matrix<double> Q { set; get; }

    /// <summary>the covariance of the observation noise</summary>
    public Matrix<double> R { set; get; }

    /// <summary>the a posteriori estimate covariance matrix</summary>
    public Matrix<double> P { set; get; }

    /// <summary>the a posteriori state estimate</summary>
    public Vector<double> X { set; get; }

    /// <summary>alternative to A and B</summary>
    public Func<TransitionVariables, Vector<double>> TransitionFunc { set; get; }

    /// <summary>alternative to C</summary>
    public Func<ObservationVariables, Vector<double>> ObservationFunc { set; get; }

    /// <summary>get next state</summary>
    /// <param name="u">(optional) control input</param>
    public double[] Predict(params double[] u);

    /// <summary>correct current state</summary>
    /// <param name="y">observation</param>
    public double[] Update(params double[] y);

}

public abstract class SequentialBayesianFilterBase : ISequentialBayesianFilter
{

    /// <summary>state vector size</summary>
    protected readonly int _k;
    /// <summary>observation vector size</summary>
    protected readonly int _m;
    /// <summary>control vector size</summary>
    protected readonly int _n;

    public double Dt { set; get; }
    public Matrix<double> A { set; get; }
    public Matrix<double> B { set; get; }
    public Matrix<double> C { set; get; }
    public Matrix<double> Q { set; get; }
    public Matrix<double> R { set; get; }
    public Matrix<double> P { set; get; }
    public Vector<double> X { set; get; }
    public Func<TransitionVariables, Vector<double>> TransitionFunc { set; get; }
    public Func<ObservationVariables, Vector<double>> ObservationFunc { set; get; }

    public SequentialBayesianFilterBase(
        IEnumerable<double> x0,
        int observationVectorSize,
        int controlVectorSize
    )
    {
        _k = x0.Count();
        _m = observationVectorSize;
        _n = controlVectorSize;
        X = DenseVector.OfEnumerable(x0);
        A = DenseMatrix.CreateIdentity(_k);
        B = DenseMatrix.CreateDiagonal(_n, _k, 1);
        C = DenseMatrix.CreateDiagonal(_m, _k, 1);
        P = DenseMatrix.CreateIdentity(_k);
        Q = DenseMatrix.CreateIdentity(_k);
        R = DenseMatrix.CreateIdentity(_m);
        TransitionFunc = v =>
        {
            if  (B.RowCount is 0 || B.ColumnCount is 0 || v.U.Count is 0)
                return A * v.X * v.Dt;
            else
                return (A * v.X + B * v.U) * v.Dt;
        };
        ObservationFunc = v => C * v.X;
    }

    public abstract double[] Predict(params double[] u);
    public abstract double[] Update(params double[] y);

}

public record struct TransitionVariables(Vector<double> X, Vector<double> U, double Dt);

public record struct ObservationVariables(Vector<double> X);

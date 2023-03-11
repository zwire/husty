using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.Filters;

public sealed class ParticleFilter : NonlinearStateFilterBase
{

    public struct Particle
    {
        public Vector<double> State { set; get; } 
        public double Likelihood { set; get; }
        public Particle(Vector<double> state, double likelihood)
        {
            State = state;
            Likelihood = likelihood;
        }
    }


    // ------ properties ------ //

    public Particle[] Particles { private set; get; }


    // ------ constructors ------ //

    /// <remarks>diagonal values of all matrix is initialized as one.</remarks>
    public ParticleFilter(
        IEnumerable<double> x0, 
        int observationVectorSize, 
        int controlVectorSize, 
        int n
    ) : base(x0, observationVectorSize, controlVectorSize)
    {
        Particles = new Particle[n];
        for (int i = 0; i < n; i++)
            Particles[i] = Randomize(new(DenseVector.OfEnumerable(x0), 1.0 / n));
    }


    // ------ public methods ------ //

    public override double[] Predict(params double[] u)
    {
        if (Dt <= 0) throw new Exception("Require: Dt > 0");
        var ps = Particles.ToArray();
        // predict next state
        for (int i = 0; i < ps.Length; i++)
        {
            var nextState = NonlinearTransitionFunction(new(ps[i].State, DenseVector.OfArray(u), Dt));
            ps[i] = Randomize(new(nextState, ps[i].Likelihood));
        }
        var state = GetWeightedState(ps);
        X = DenseVector.OfArray(GetWeightedState(ps));
        Particles = ps;
        return state;
    }

    public override double[] Update(params double[] y)
    {
        var ps = Particles.ToArray();
        // update likelihood
        var sum = 0.0;
        var best = double.MaxValue;
        var bestIndex = 0;
        for (int i = 0; i < ps.Length; i++)
        {
            var diff = (DenseVector.OfArray(y) - NonlinearObservationFunction(new(ps[i].State))).ToRowMatrix();
            var e = (diff * R.Inverse() * diff.Transpose())[0, 0];
            var ex = Math.Exp(-0.5 * e);
            ps[i].Likelihood = ex;
            sum += ex;
            if (e < best)
            {
                best = e;
                bestIndex = i;
            }
        }
        if (sum is 0)
        {
            ps[bestIndex].Likelihood = 1;
        }
        NormalizeLikelihood(ref ps);
        var state = GetWeightedState(ps);
        X = DenseVector.OfArray(state);
        // resampling to keep the important particles alive
        var ps2 = new List<Particle>();
        var sum2 = 0.0;
        var j = 0;
        var scale = 1.0 / ps.Length;
        for (var d = scale / 2; d < 1; d += scale)
        {
            while (sum2 + ps[j].Likelihood < d)
            {
                sum2 += ps[j].Likelihood;
                j++;
            }
            ps2.Add(ps[j]);
        }
        ps = ps2.ToArray();
        NormalizeLikelihood(ref ps);
        Particles = ps;
        return state;
    }


    // ------ private methods ------ //

    private Particle Randomize(Particle p)
    {
        var state = new DenseVector(p.State.Count);
        for (int i = 0; i < _k; i++)
            state[i] = Normal.Sample(p.State[i], Math.Sqrt(Q[i, i]));
        return new(state, p.Likelihood);
    }

    private double[] GetWeightedState(Particle[] ps)
    {
        var state = new double[_k];
        for (int i = 0; i < ps.Length; i++)
            for (int j = 0; j < _k; j++)
                state[j] += ps[i].State[j] * ps[i].Likelihood;
        return state;
    }

    private static void NormalizeLikelihood(ref Particle[] ps)
    {
        var sum = 0.0;
        for (int i = 0; i < ps.Length; i++)
            sum += ps[i].Likelihood;
        for (int i = 0; i < ps.Length; i++)
            ps[i].Likelihood = ps[i].Likelihood / sum;
    }

}

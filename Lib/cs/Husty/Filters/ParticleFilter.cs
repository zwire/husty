using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.Filters;

public sealed class ParticleFilter : SequentialBayesianFilterBase
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

    public Func<Particle[], Particle[]> ResamplingFunc { set; get; }

    public Func<Vector<double>, double> GetLikelihoodFunc { set; get; }


    // ------ constructors ------ //

    /// <remarks>diagonal values of all matrix is initialized as one.</remarks>
    public ParticleFilter(
        IEnumerable<double> x0, 
        int observationVectorSize, 
        int controlVectorSize, 
        int n
    ) : base(x0, observationVectorSize, controlVectorSize)
    {
        Particles = Enumerable.Range(0, n).Select(_ => new Particle(DenseVector.OfEnumerable(x0), 1.0 / n)).ToArray();
        Randomize(Particles);
        // systematic resampling
        ResamplingFunc = ps =>
        {
            var tmp = new List<Particle>();
            var sum = 0.0;
            var i = 0;
            var scale = 1.0 / ps.Length;
            for (var d = scale / 2; d < 1; d += scale)
            {
                while (sum + ps[i].Likelihood < d)
                {
                    sum += ps[i].Likelihood;
                    i++;
                }
                tmp.Add(ps[i]);
            }
            return tmp.ToArray();
        };
        // fitting a Gaussian distribution
        GetLikelihoodFunc = e =>
        {
            return Math.Exp(-0.5 * e * R.Inverse() * e);
        };
    }


    // ------ public methods ------ //

    public override double[] Predict(params double[] u)
    {
        if (Dt <= 0) throw new Exception("Require: Dt > 0");
        var ps = Particles.ToArray();
        // predict next state
        for (int i = 0; i < ps.Length; i++)
            ps[i].State = TransitionFunc(new(ps[i].State, DenseVector.OfArray(u), Dt));
        Randomize(ps);
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
        var index = 0;
        for (int i = 0; i < ps.Length; i++)
        {
            var e = DenseVector.OfArray(y) - ObservationFunc(new(ps[i].State));
            ps[i].Likelihood = GetLikelihoodFunc(e);
            sum += ps[i].Likelihood;
            var norm = e * e;
            if (norm < best)
            {
                index = i;
                best = norm;
            }
        }
        if (sum is 0) ps[index].Likelihood = 1;
        NormalizeLikelihood(ps);
        var state = GetWeightedState(ps);
        X = DenseVector.OfArray(state);
        ps = ResamplingFunc(ps);
        NormalizeLikelihood(ps);
        Particles = ps;
        return state;
    }


    // ------ private methods ------ //

    private void Randomize(Particle[] ps)
    {
        for (int i = 0; i < ps.Length; i++)
            for (int j = 0; j < _k; j++)
                ps[i].State[j] += Q.Row(j).Select(q => Normal.Sample(0, Math.Sqrt(q))).Sum();
    }

    private double[] GetWeightedState(Particle[] ps)
    {
        var state = new double[_k];
        for (int i = 0; i < ps.Length; i++)
            for (int j = 0; j < _k; j++)
                state[j] += ps[i].State[j] * ps[i].Likelihood;
        return state;
    }

    private static void NormalizeLikelihood(Particle[] ps)
    {
        var sum = 0.0;
        for (int i = 0; i < ps.Length; i++)
            sum += ps[i].Likelihood;
        for (int i = 0; i < ps.Length; i++)
            ps[i].Likelihood = ps[i].Likelihood / sum;
    }

}

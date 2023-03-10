using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.Filters;

public sealed class ParticleFilter : NonlinearStateFilterBase
{

    public record struct Particle(Vector<double> State, double Likelihood);


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
            Particles[i] = Randmize(new(DenseVector.OfEnumerable(x0), 1.0 / n));
    }


    // ------ public methods ------ //

    public override double[] Predict(params double[] u)
    {
        if (Dt <= 0) throw new Exception("Require: Dt > 0");
        // predict next state
        for (int i = 0; i < Particles.Length; i++)
        {
            var nextState = NonlinearTransitionFunction(new(Particles[i].State, DenseVector.OfArray(u), Dt));
            Particles[i] = Randmize(new(nextState, Particles[i].Likelihood));
        }
        // approximate next state as weighted average
        var state = new double[_k];
        for (int i = 0; i < Particles.Length; i++)
            for (int j = 0; j < _k; j++)
                state[j] += Particles[i].State[j] * Particles[i].Likelihood;
        return state;
    }

    public override double[] Update(params double[] y)
    {
        // update likelihood
        var sum = 0.0;
        var likelihoods = new double[Particles.Length];
        for (int i = 0; i < Particles.Length; i++)
        {
            // TODO: likelihood design
            var e = DenseVector.OfArray(y) - NonlinearObservationFunction(new(Particles[i].State));
            likelihoods[i] = 1.0 / Math.Exp((e * e * R).L2Norm());
            sum += likelihoods[i];
        }
        for (int i = 0; i < Particles.Length; i++)
        {
            Particles[i] = new(Particles[i].State, likelihoods[i] / sum);
        }
        // approximate current state as weighted average
        var state = new double[_k];
        for (int i = 0; i < Particles.Length; i++)
            for (int j = 0; j < _k; j++)
                state[j] += Particles[i].State[j] * Particles[i].Likelihood;
        // resampling to keep the important particles alive.
        var tmp = new List<Particle>();
        var scale = 1.0 / Particles.Length;
        for (var d = scale / 2; d < 1; d += scale)
        {
            var sum2 = 0.0;
            for (int i = 0; i < Particles.Length; i++)
            {
                sum2 += Particles[i].Likelihood;
                if (sum2 > d)
                {
                    tmp.Add(Particles[i]);
                    break;
                }
            }
        }
        Particles = tmp.ToArray();
        return state;
    }


    // ------ private methods ------ //

    private Particle Randmize(Particle p)
    {
        var state = new DenseVector(p.State.Count);
        for (int i = 0; i < _k; i++)
            state[i] = Normal.Sample(p.State[i], Q[i, i]);
        return new(state, p.Likelihood);
    }

}

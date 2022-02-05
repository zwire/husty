using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public interface ITunableLayer : ILayer
    {

        public Matrix<double> W { get; }

        public Vector<double> B { get; }

        public Matrix<double> GradW { get; }

        public Vector<double> GradB { get; }

        public IOptimizer Optimizer { get; }

        public void SetParams(Matrix<double> w, Vector<double> b);

    }
}

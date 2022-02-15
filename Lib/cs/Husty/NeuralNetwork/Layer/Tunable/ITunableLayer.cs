using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public interface ITunableLayer : ILayer
    {

        public Matrix<float> W { get; }

        public Vector<float> B { get; }

        public Matrix<float> GradW { get; }

        public Vector<float> GradB { get; }

        public IOptimizer Optimizer { get; }

        public void SetParams(Matrix<float> w, Vector<float> b);

        public void Optimize();

    }
}

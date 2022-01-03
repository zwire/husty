using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public interface ITunableLayer : ILayer
    {

        public IOptimizer Optimizer { get; }

        public void SetParams(Matrix<double> W, Vector<double> B);

        public (Matrix<double> W, Vector<double> B) GetParams();

        public (Matrix<double> GradW, Vector<double> GradB) GetGradients();

    }
}

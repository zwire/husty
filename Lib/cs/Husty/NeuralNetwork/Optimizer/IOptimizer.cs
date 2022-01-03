using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public interface IOptimizer
    {

        public Matrix<double> Weight { get; }

        public Vector<double> Bias { get; }

        public (Matrix<double> W, Vector<double> B) Update(Matrix<double> w, Vector<double> b, Matrix<double> gw, Vector<double> gb);

    }
}

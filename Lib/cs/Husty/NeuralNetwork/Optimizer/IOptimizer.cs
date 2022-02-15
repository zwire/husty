using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public interface IOptimizer
    { 

        public (Matrix<float> W, Vector<float> B) Update(Matrix<float> w, Vector<float> b, Matrix<float> gw, Vector<float> gb);

    }
}

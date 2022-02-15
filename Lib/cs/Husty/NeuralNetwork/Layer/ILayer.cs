using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public interface ILayer
    {

        public Matrix<float> Forward(Matrix<float> x);

        public Matrix<float> Backward(Matrix<float> dout);

        public string Serialize();

    }
}

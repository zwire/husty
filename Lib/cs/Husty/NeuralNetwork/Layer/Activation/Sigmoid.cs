using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public class Sigmoid : IActivationLayer
    {

        private Matrix<float> _y;

        public Matrix<float> Forward(Matrix<float> x)
        {
            _y = 1.0f / (1.0f + (-x).PointwiseExp());
            return _y;
        }

        public Matrix<float> Backward(Matrix<float> dout)
        {
            return (1.0f - _y) * _y.Transpose() * dout;
        }

        public string Serialize()
        {
            return "Sigmoid";
        }

        internal static ILayer Deserialize(string[] line)
        {
            return new Sigmoid();
        }

    }
}

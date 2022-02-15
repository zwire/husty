using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public class Tanh : IActivationLayer
    {

        private Matrix<float> _y;

        public Matrix<float> Forward(Matrix<float> x)
        {
            _y = x.PointwiseTanh();
            return _y;
        }

        public Matrix<float> Backward(Matrix<float> dout)
        {
            return (1.0f - _y * _y.Transpose()) * dout;
        }

        public string Serialize()
        {
            return "Tanh";
        }

        internal static ILayer Deserialize(string[] line)
        {
            return new Tanh();
        }

    }
}

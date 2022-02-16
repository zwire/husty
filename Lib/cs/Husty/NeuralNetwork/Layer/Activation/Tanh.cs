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
            return dout.PointwiseMultiply(1f - _y.PointwisePower(2));
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

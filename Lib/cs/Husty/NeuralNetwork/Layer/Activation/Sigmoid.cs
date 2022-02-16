using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public class Sigmoid : IActivationLayer
    {

        private Matrix<float> _y;

        public Matrix<float> Forward(Matrix<float> x)
        {
            _y = 1f / (1f + (-x).PointwiseExp());
            return _y;
        }

        public Matrix<float> Backward(Matrix<float> dout)
        {
            return dout.PointwiseMultiply(1f - _y.PointwisePower(2));
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

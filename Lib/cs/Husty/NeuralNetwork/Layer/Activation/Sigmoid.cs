using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public class Sigmoid : IActivationLayer
    {

        private Vector<float> _y;

        public Vector<float> Forward(Vector<float> x)
        {
            _y = 1f / (1f + (-x).PointwiseExp());
            return _y;
        }

        public Vector<float> Backward(Vector<float> dout)
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

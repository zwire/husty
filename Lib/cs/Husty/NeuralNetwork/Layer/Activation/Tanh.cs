using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public class Tanh : IActivationLayer
    {

        private Vector<float> _y;

        public Vector<float> Forward(Vector<float> x)
        {
            _y = x.PointwiseTanh();
            return _y;
        }

        public Vector<float> Backward(Vector<float> dout)
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

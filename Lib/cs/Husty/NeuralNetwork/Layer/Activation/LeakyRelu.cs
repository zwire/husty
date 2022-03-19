using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Husty.NeuralNetwork
{
    public class LeakyRelu : IActivationLayer
    {

        private Vector<float> _y;
        private readonly float _alpha;

        public LeakyRelu(float alpha = 0.01f) 
        {
            _alpha = alpha;
        }

        public Vector<float> Forward(Vector<float> x)
        {
            _y = x.Map(p => p > 0 ? p : _alpha * p);
            return _y;
        }

        public Vector<float> Backward(Vector<float> dout)
        {
            var dx = new DenseVector(dout.Count);
            for (int i = 0; i < dout.Count; i++)
                dx[i] = _y[i] > 0 ? dout[i] : _alpha * dout[i];
            return dx;
        }

        public string Serialize()
        {
            return $"LeakyRelu::{_alpha}";
        }

        internal static ILayer Deserialize(string[] line)
        {
            return new LeakyRelu(float.Parse(line[0]));
        }

    }
}

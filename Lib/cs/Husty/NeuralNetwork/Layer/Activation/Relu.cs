using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.NeuralNetwork
{
    public class Relu : IActivationLayer
    {

        private Vector<double> _y;

        public Vector<double> Forward(Vector<double> x)
        {
            _y = new DenseVector(x.Count);
            for (int i = 0; i < x.Count; i++)
                _y[i] = x[i] > 0 ? x[i] : 0;
            return _y;
        }

        public Vector<double> Backward(Vector<double> dout, bool freeze = false)
        {
            var dx = new DenseVector(dout.Count);
            for (int i = 0; i < dout.Count; i++)
                dx[i] = _y[i] > 0 ? dout[i] : 0;
            return dx;
        }

    }
}

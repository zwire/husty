using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.NeuralNetwork
{
    public class Relu : IActivationLayer
    {

        private DenseVector _y;

        public DenseVector Forward(DenseVector x)
        {
            _y = new(x.Count);
            for (int i = 0; i < x.Count; i++)
                _y[i] = x[i] > 0 ? x[i] : 0;
            return _y;
        }

        public DenseVector Backward(DenseVector dout)
        {
            DenseVector dx = new(dout.Count);
            for (int i = 0; i < dout.Count; i++)
                dx[i] = _y[i] > 0 ? dout[i] : 0;
            return dx;
        }

    }
}

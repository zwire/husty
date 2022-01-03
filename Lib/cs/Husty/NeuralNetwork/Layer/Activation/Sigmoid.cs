using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.NeuralNetwork
{
    public class Sigmoid : IActivationLayer
    {

        private DenseVector y;

        public DenseVector Forward(DenseVector x)
        {
            y = new(x.Count);
            for (int i = 0; i < x.Count; i++)
            {
                y[i] = 1.0 / (1.0 + Math.Exp(-x[i]));
            }
            return y;
        }

        public DenseVector Backward(DenseVector dout)
        {
            DenseVector dx = new(dout.Count);
            for (int i = 0; i < dout.Count; i++)
            {
                dx[i] = dout[i] * (1 - y[i]) * y[i];
            }
            return dx;
        }

    }
}

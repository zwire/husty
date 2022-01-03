using System;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.NeuralNetwork
{
    public class Tanh : IActivationLayer
    {

        private DenseVector y;

        public DenseVector Forward(DenseVector x)
        {
            y = new(x.Count);
            for (int i = 0; i < x.Count; i++)
            {
                y[i] = Math.Tanh(x[i]);
            }
            return y;
        }

        public DenseVector Backward(DenseVector dout)
        {
            DenseVector dx = new(dout.Count);
            for (int i = 0; i < dout.Count; i++)
            {
                dx[i] = 1 - y[i] * y[i];
            }
            return dx;
        }

    }
}

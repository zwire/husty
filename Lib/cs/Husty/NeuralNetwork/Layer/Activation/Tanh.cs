using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.NeuralNetwork
{
    public class Tanh : IActivationLayer
    {

        private Vector<double> y;

        public Vector<double> Forward(Vector<double> x)
        {
            y = new DenseVector(x.Count);
            for (int i = 0; i < x.Count; i++)
            {
                y[i] = Math.Tanh(x[i]);
            }
            return y;
        }

        public Vector<double> Backward(Vector<double> dout, bool freeze = false)
        {
            var dx = new DenseVector(dout.Count);
            for (int i = 0; i < dout.Count; i++)
            {
                dx[i] = 1 - y[i] * y[i];
            }
            return dx;
        }

    }
}

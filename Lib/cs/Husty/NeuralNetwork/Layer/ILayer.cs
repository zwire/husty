using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public interface ILayer
    {

        public Vector<double> Forward(Vector<double> x);

        public Vector<double> Backward(Vector<double> dout, bool freeze);

    }
}

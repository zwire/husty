using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.NeuralNetwork
{
    public interface ILayer
    {

        public DenseVector Forward(DenseVector x);

        public DenseVector Backward(DenseVector dout);

    }
}

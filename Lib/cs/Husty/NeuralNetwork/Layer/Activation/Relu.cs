using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Husty.NeuralNetwork
{
    public class Relu : IActivationLayer
    {

        private Matrix<float> _y;

        public Matrix<float> Forward(Matrix<float> x)
        {
            _y = x.Map(p => p > 0 ? p : 0);
            return _y;
        }

        public Matrix<float> Backward(Matrix<float> dout)
        {
            var dx = new DenseMatrix(dout.RowCount, dout.ColumnCount);
            for (int i = 0; i < dout.RowCount; i++)
                for (int j = 0; j < dout.ColumnCount; j++)
                    dx[i, j] = _y[i, j] > 0 ? dout[i, j] : 0;
            return dx;
        }

        public string Serialize()
        {
            return "Relu";
        }

        internal static ILayer Deserialize(string[] line)
        {
            return new Relu();
        }

    }
}

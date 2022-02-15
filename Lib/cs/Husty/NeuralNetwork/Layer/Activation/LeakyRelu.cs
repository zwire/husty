using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Husty.NeuralNetwork
{
    public class LeakyRelu : IActivationLayer
    {

        private Matrix<float> _y;
        private readonly float _alpha;

        public LeakyRelu(float alpha = 0.01f) 
        {
            _alpha = alpha;
        }

        public Matrix<float> Forward(Matrix<float> x)
        {
            _y = x.Map(p => p > 0 ? p : _alpha * p);
            return _y;
        }

        public Matrix<float> Backward(Matrix<float> dout)
        {
            var dx = new DenseMatrix(dout.RowCount, dout.ColumnCount);
            for (int i = 0; i < dout.RowCount; i++)
                for (int j = 0; j < dout.ColumnCount; j++)
                    dx[i, j] = _y[i, j] > 0 ? dout[i, j] : _alpha * dout[i, j];
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

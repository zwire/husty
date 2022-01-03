using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.NeuralNetwork
{
    // Y = W * X + B
    //
    // 入力 ... m
    // 出力 ... n
    // の場合、
    // W ... m * n
    // B ... 1 * n (Vector)
    public class Affine : ITunableLayer
    {

        private DenseVector _x;
        private DenseMatrix _gradW;
        private DenseVector _gradB;
        private DenseMatrix _w;
        private DenseVector _b;

        public IOptimizer Optimizer { get; }


        public Affine(IOptimizer opt, int inshape, int outshape)
        {
            Optimizer = opt;
            _w = new(inshape, outshape);
            for (int i = 0; i < _w.RowCount; i++)
            {
                for (int j = 0; j < _w.ColumnCount; j++)
                {
                    var rnd = new Random();
                    _w[i, j] = (rnd.NextDouble() - 0.5) / 100;
                }
            }
            _b = new(outshape);
            for (int i = 0; i < _b.Count; i++)
            {
                var rnd = new Random();
                _b[i] = rnd.NextDouble() - 0.5;
            }
        }

        public Affine(IOptimizer opt, DenseMatrix weights, DenseVector bias)
        {
            Optimizer = opt;
            _w = weights;
            _b = bias;
        }

        public void SetParams(Matrix<double> W, Vector<double> B)
        {
            _w = (DenseMatrix)W;
            _b = (DenseVector)B;
        }

        public (Matrix<double> W, Vector<double> B) GetParams()
        {
            return (_w, _b);
        }

        public (Matrix<double> GradW, Vector<double> GradB) GetGradients()
        {
            return (_gradW, _gradB);
        }

        public DenseVector Forward(DenseVector x)
        {
            _x = x;
            return _x * _w + _b;
        }

        public DenseVector Backward(DenseVector dout)
        {
            _gradW = (DenseMatrix)(_x.ToColumnMatrix() * dout.ToRowMatrix());
            _gradB = dout;
            return (dout.ToRowMatrix() * _w.Transpose()).ToRowMajorArray();
        }

    }
}

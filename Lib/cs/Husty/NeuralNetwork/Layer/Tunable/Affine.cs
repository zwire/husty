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

        private Vector<double> _x;
        private Matrix<double> _gradW;
        private Vector<double> _gradB;
        private Matrix<double> _w;
        private Vector<double> _b;

        public Matrix<double> W => _w;

        public Vector<double> B => _b;

        public Matrix<double> GradW => _gradW;

        public Vector<double> GradB => _gradB;

        public IOptimizer Optimizer { get; }


        public Affine(IOptimizer opt, int inshape, int outshape)
        {
            Optimizer = opt;
            _w = new DenseMatrix(inshape, outshape);
            for (int i = 0; i < _w.RowCount; i++)
            {
                for (int j = 0; j < _w.ColumnCount; j++)
                {
                    var rnd = new Random();
                    _w[i, j] = (rnd.NextDouble() - 0.5) / 100;
                }
            }
            _b = new DenseVector(outshape);
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

        public Vector<double> Forward(Vector<double> x)
        {
            _x = x;
            return _x * _w + _b;
        }

        public Vector<double> Backward(Vector<double> dout, bool freeze)
        {
            _gradW = (DenseMatrix)(_x.ToColumnMatrix() * dout.ToRowMatrix());
            _gradB = dout;
            if (!freeze)
                (_w, _b) = Optimizer.Update(_w, _b, _gradW, _gradB);
            return dout * _w.Transpose();
        }

        public void SetParams(Matrix<double> w, Vector<double> b)
        {
            _w = w;
            _b = b;
        }

    }
}

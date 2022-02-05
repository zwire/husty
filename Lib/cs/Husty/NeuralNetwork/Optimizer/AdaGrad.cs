using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.NeuralNetwork
{
    public class AdaGrad : OptimizerBase
    {

        private readonly double _rate;
        private Matrix<double> _hw;
        private Vector<double> _hb;

        public AdaGrad(double rate = 0.01)
        {
            _rate = rate;
        }

        protected override Matrix<double> Optimize(Matrix<double> w, Matrix<double> gw)
        {
            if (_hw is null) _hw = new DenseMatrix(w.RowCount, w.ColumnCount);
            for (int k = 0; k < w.RowCount; k++)
            {
                for (int l = 0; l < w.ColumnCount; l++)
                {
                    _hw[k, l] += Math.Pow(gw[k, l], 2);
                    w[k, l] -= _rate * gw[k, l] / (Math.Sqrt(_hw[k, l]) + 1e-7);
                }
            }
            return w;
        }

        protected override Vector<double> Optimize(Vector<double> b, Vector<double> gb)
        {
            if (_hb is null) _hb = new DenseVector(b.Count);
            for (int m = 0; m < b.Count; m++)
            {
                _hb[m] += Math.Pow(gb[m], 2);
                b[m] -= _rate * gb[m] / (Math.Sqrt(_hb[m]) + 1e-7);
            }
            return b;
        }

    }
}

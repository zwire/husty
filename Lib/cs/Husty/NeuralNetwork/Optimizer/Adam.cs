using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.NeuralNetwork
{
    public class Adam : OptimizerBase
    {

        private readonly double _rate;
        private readonly double _alpha;
        private readonly double _beta;
        private readonly double _1_alpha;
        private readonly double _1_beta;
        private double _rate_t;
        private int _itr;
        private DenseMatrix _mw;
        private DenseMatrix _vw;
        private DenseVector _mb;
        private DenseVector _vb;

        public Adam(double rate = 0.01, double alpha = 0.9, double beta = 0.999)
        {
            _rate = rate;
            _alpha = alpha;
            _beta = beta;
            _1_alpha = 1.0 - _alpha;
            _1_beta = 1.0 - _beta;
        }

        protected override Matrix<double> Optimize(Matrix<double> w, Matrix<double> gw)
        {
            if (_mw is null)
            {
                _mw = new DenseMatrix(w.RowCount, w.ColumnCount);
                _vw = new DenseMatrix(w.RowCount, w.ColumnCount);
            }
            _itr++;
            _rate_t = _rate * Math.Sqrt(1.0 - Math.Pow(_beta, _itr)) / (1.0 - Math.Pow(_alpha, _itr));
            var wClone = w.Clone();
            for (int k = 0; k < w.RowCount; k++)
            {
                for (int l = 0; l < w.ColumnCount; l++)
                {
                    _mw[k, l] += _1_alpha * (gw[k, l] - _mw[k, l]);
                    _vw[k, l] += _1_beta * (Math.Pow(gw[k, l], 2) - _vw[k, l]);
                    wClone[k, l] -= _rate_t * _mw[k, l] / (Math.Sqrt(_vw[k, l]) + 1e-7);
                }
            }
            return wClone;
        }

        protected override Vector<double> Optimize(Vector<double> b, Vector<double> gb)
        {
            if (_mb is null)
            {
                _mb = new double[b.Count];
                _vb = new double[b.Count];
            }
            var bClone = b.Clone();
            for (int m = 0; m < b.Count; m++)
            {
                _mb[m] += _1_alpha * (gb[m] - _mb[m]);
                _vb[m] += _1_beta * (Math.Pow(gb[m], 2) - _vb[m]);
                bClone[m] -= _rate_t * _mb[m] / (Math.Sqrt(_vb[m]) + 1e-7);
            }
            return bClone;
        }

    }
}

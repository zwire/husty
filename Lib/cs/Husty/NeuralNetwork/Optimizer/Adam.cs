using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Husty.NeuralNetwork
{
    public class Adam : OptimizerBase
    {

        // ------ fields ------ //

        private readonly float _rate;
        private readonly float _alpha;
        private readonly float _beta;
        private int _itrW;
        private int _itrB;
        private Matrix<float> _mw;
        private Matrix<float> _vw;
        private Vector<float> _mb;
        private Vector<float> _vb;


        // ------ properties ------ //

        public float Rate => _rate;

        public float Alpha => _alpha;

        public float Beta => _beta;


        // ------ constructors ------ //

        public Adam(float rate = 0.001f, float alpha = 0.9f, float beta = 0.999f)
        {
            _rate = rate;
            _alpha = alpha;
            _beta = beta;
        }


        // ------ override methods ------ //

        protected override Matrix<float> Optimize(Matrix<float> w, Matrix<float> gw)
        {
            if (_mw is null)
            {
                _mw = new DenseMatrix(w.RowCount, w.ColumnCount);
                _vw = new DenseMatrix(w.RowCount, w.ColumnCount);
            }
            _itrW++;
            var rate_t = _rate * (float)Math.Sqrt(1.0 - Math.Pow(_beta, _itrW)) / (float)(1.0 - Math.Pow(_alpha, _itrW));
            _mw += (1.0f - _alpha) * (gw - _mw);
            _vw += (1.0f - _beta) * (gw.PointwisePower(2) - _vw);
            return w - rate_t * _mw.PointwiseDivide(_vw.PointwiseSqrt() + 1e-8f);
        }

        protected override Vector<float> Optimize(Vector<float> b, Vector<float> gb)
        {
            if (_mb is null)
            {
                _mb = DenseVector.OfArray(new float[b.Count]);
                _vb = DenseVector.OfArray(new float[b.Count]);
            }
            _itrB++;
            var rate_t = _rate * (float)Math.Sqrt(1.0 - Math.Pow(_beta, _itrB)) / (float)(1.0 - Math.Pow(_alpha, _itrB));
            _mb += (1.0f - _alpha) * (gb - _mb);
            _vb += (1.0f - _beta) * (gb.PointwisePower(2) - _vb);
            return b - rate_t * _mb / (_vb.PointwiseSqrt() + 1e-8f);
        }

    }
}

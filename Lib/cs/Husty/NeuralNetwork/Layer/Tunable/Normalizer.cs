using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.NeuralNetwork
{
    public class Normalizer : ITunableLayer
    {

        private double _sigma;
        private double _b;
        private double _w;
        private double _gradW;
        private double _gradB;
        private DenseVector _xc;
        private DenseVector _y;


        public IOptimizer Optimizer { get; }


        public Normalizer(IOptimizer opt, double gamma = 1, double beta = 0)
        {
            Optimizer = opt;
            _w = gamma;
            _b = beta;
        }

        public DenseVector Forward(DenseVector x)
        {
            _xc = new(x.Count);
            _y = new(x.Count);
            var mean = x.Average();
            _sigma = 0.0;
            for (int i = 0; i < x.Count; i++)
                _sigma += Math.Pow(x[i] - mean, 2);
            _sigma /= x.Count;
            _sigma = Math.Sqrt(_sigma + 1e-7);
            for (int i = 0; i < x.Count; i++)
            {
                _xc[i] = x[i] - mean;
                _y[i] = _w * _xc[i] / _sigma - _b;
            }
            return _y;
        }

        public DenseVector Backward(DenseVector dout)
        {
            var dx = dout / _sigma;
            _gradB = dout[0];
            _gradW = _xc[0] * dout[0];
            double dsigma = -_w * dout * _xc / _sigma / _sigma;
            dx += _xc * dsigma / _sigma;
            dx = _y - dx;
            return dx;
        }

        public void SetParams(Matrix<double> W, Vector<double> B)
        {
            _w = W.ToRowMajorArray()[0];
            _b = B.ToArray()[0];
        }

        public (Matrix<double> W, Vector<double> B) GetParams()
        {
            return (DenseMatrix.OfArray(new[,] { { _w } }), DenseVector.OfArray(new[] { _b }));
        }

        public (Matrix<double> GradW, Vector<double> GradB) GetGradients()
        {
            return (DenseMatrix.OfArray(new[,] { { _gradW } }), DenseVector.OfArray(new[] { _gradB }));
        }

    }
}

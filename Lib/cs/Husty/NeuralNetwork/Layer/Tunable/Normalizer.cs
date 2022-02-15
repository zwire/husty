using System;
using System.Linq;
using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Husty.NeuralNetwork
{
    // TODO: test
    // see https://www.anarchive-beta.com/entry/2020/08/17/180000
    public class Normalizer : ITunableLayer
    {

        // ------ fields ------ //

        private Vector<float> _sigmas;
        private Matrix<float> _xc;
        private Matrix<float> _y;


        // ------ properties ------ //

        public Matrix<float> W { private set; get; }

        public Vector<float> B { private set; get; }

        public Matrix<float> GradW { private set; get; }

        public Vector<float> GradB { private set; get; }

        public IOptimizer Optimizer { get; }


        // ------ constructors ------ //

        public Normalizer(IOptimizer optimizer, float gamma = 1, float beta = 0)
        {
            Optimizer = optimizer;
            W = DenseMatrix.OfArray(new[,] { { gamma } });
            B = DenseVector.OfArray(new[] { beta });
        }

        public Normalizer(IOptimizer optimizer, DenseMatrix weights, DenseVector bias)
        {
            Optimizer = optimizer;
            W = weights;
            B = bias;
        }


        // ------ public methods ------ //

        public Matrix<float> Forward(Matrix<float> x)
        {
            _y = new DenseMatrix(x.RowCount, x.ColumnCount);
            var means = x.ColumnSums() / x.ColumnCount;
            _sigmas = new DenseVector(x.RowCount);
            for (int i = 0; i < x.RowCount; i++)
                for (int j = 0; j < x.ColumnCount; j++)
                    _sigmas[j] += (float)Math.Pow(x[i, j] - means[j], 2);
            _sigmas /= x.ColumnCount;
            _sigmas = (_sigmas + 1e-7f).PointwiseSqrt();
            _xc = x - means.ToColumnMatrix();
            for (int i = 0; i < x.RowCount; i++)
                for (int j = 0; j < x.ColumnCount; j++)
                    _xc[i, j] /= _sigmas[j];
            var bMat = Matrix<float>.Build.DenseOfRows(Enumerable.Repeat(B, x.RowCount));
            _y = W * _xc + bMat;
            return _y;
        }

        public Matrix<float> Backward(Matrix<float> dout)
        {
            var dx = new DenseMatrix(dout.RowCount, dout.ColumnCount);
            for (int i = 0; i < dout.RowCount; i++)
                for (int j = 0; j < dout.ColumnCount; j++)
                    dx[i, j] = dout[i, j] / _sigmas[j];
            GradW = dout.PointwiseMultiply(_xc).ColumnSums().ToColumnMatrix();
            GradB = dout.ColumnSums();
            Matrix<float> dl = new DenseMatrix(dout.RowCount, dout.ColumnCount);
            for (int j = 0; j < dout.ColumnCount; j++)
                for (int i = 0; i < dout.RowCount; i++)
                    dl[i, j] = dx[i, j] * (1 - (float)Math.Pow(_xc[i, j], 2) / dout.RowCount) / _sigmas[j];
            dl -= dl / dout.RowCount;
            return dl;
        }

        public void SetParams(Matrix<float> w, Vector<float> b)
        {
            W = w;
            B = b;
        }

        public void Optimize()
        {
            (W, B) = Optimizer.Update(W, B, GradW, GradB);
        }

        public string Serialize()
        {
            var opt = JsonSerializer.Serialize<object>(Optimizer);
            var w = JsonSerializer.Serialize(W.ToRowArrays());
            var b = JsonSerializer.Serialize(B.ToArray());
            return $"Nomalizer::{opt}::{w}::{b}";
        }

        public static ILayer Deserialize(string[] line)
        {
            var opt = JsonSerializer.Deserialize<object>(line[0]) as IOptimizer;
            var w = JsonSerializer.Deserialize<float[][]>(line[1]);
            var b = JsonSerializer.Deserialize<float[]>(line[2]);
            return new Normalizer(opt, DenseMatrix.OfRowArrays(w), DenseVector.OfArray(b));
        }

    }
}

using System;
using System.Linq;
using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Husty.NeuralNetwork
{
    public class Affine : ITunableLayer
    {

        // ------ fields ------ //

        private Matrix<float> _x;


        // ------ properties ------ //

        public Matrix<float> W { private set; get; }

        public Vector<float> B { private set; get; }

        public Matrix<float> GradW { private set; get; }

        public Vector<float> GradB { private set; get; }

        public IOptimizer Optimizer { get; }


        // ------ constructors ------ //

        public Affine(IOptimizer optimizer, int inshape, int outshape)
        {
            Optimizer = optimizer;
            W = new DenseMatrix(inshape, outshape);
            for (int i = 0; i < W.RowCount; i++)
            {
                for (int j = 0; j < W.ColumnCount; j++)
                {
                    var rnd = new Random();
                    W[i, j] = (float)(rnd.NextDouble() - 0.5f);
                }
            }
            B = new DenseVector(outshape);
            for (int i = 0; i < B.Count; i++)
            {
                var rnd = new Random();
                B[i] = (float)(rnd.NextDouble() - 0.5f);
            }
        }

        public Affine(IOptimizer optimizer, DenseMatrix weights, DenseVector bias)
        {
            Optimizer = optimizer;
            W = weights;
            B = bias;
        }


        // ------ public methods ------ //

        public Matrix<float> Forward(Matrix<float> x)
        {
            _x = x;
            var bMat = Matrix<float>.Build.DenseOfRows(Enumerable.Repeat(B, x.RowCount));
            return _x * W + bMat;
        }

        public Matrix<float> Backward(Matrix<float> dout)
        {
            GradW = _x.Transpose() * dout;
            GradB = dout.ColumnSums();
            return dout * W.Transpose();
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
            var opt = JsonSerializer.Serialize(Optimizer, Optimizer.GetType());
            var w = JsonSerializer.Serialize(W.ToRowArrays());
            var b = JsonSerializer.Serialize(B.ToArray());
            return $"Affine::{opt}::{w}::{b}";
        }

        internal static ILayer Deserialize(string[] line)
        {
            var opt = JsonSerializer.Deserialize<object>(line[0]) as IOptimizer;
            var w = JsonSerializer.Deserialize<float[][]>(line[1]);
            var b = JsonSerializer.Deserialize<float[]>(line[2]);
            return new Affine(opt, DenseMatrix.OfRowArrays(w), DenseVector.OfArray(b));
        }

    }
}

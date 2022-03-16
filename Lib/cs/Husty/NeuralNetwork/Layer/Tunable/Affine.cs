using System;
using System.Linq;
using System.Text.Json;
using MathNet.Numerics.Distributions;
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
            var normal = new Normal(0, Math.Sqrt(1.0 / inshape));
            W = new DenseMatrix(inshape, outshape, Enumerable.Range(0, inshape * outshape).Select(_ => (float)normal.Sample()).ToArray());
            B = new DenseVector(outshape);
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

        public void Optimize()
        {
            (W, B) = Optimizer.Update(W, B, GradW, GradB);
        }

        public string Serialize()
        {
            var opt = Optimizer.Serialize();
            var w = JsonSerializer.Serialize(W.ToRowArrays());
            var b = JsonSerializer.Serialize(B.ToArray());
            return $"Affine::{opt}::{w}::{b}";
        }

        internal static ILayer Deserialize(string[] line)
        {
            var opt = OptimizerFactory.Deserialize(line[0]);
            var w = JsonSerializer.Deserialize<float[][]>(line[1]);
            var b = JsonSerializer.Deserialize<float[]>(line[2]);
            return new Affine(opt, DenseMatrix.OfRowArrays(w), DenseVector.OfArray(b));
        }

    }
}

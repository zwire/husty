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

        private Vector<float> _x;


        // ------ properties ------ //

        public Matrix<float> W { private set; get; }

        public Vector<float> B { private set; get; }

        public float[,] GradW { private set; get; }

        public float[] GradB { private set; get; }

        public IOptimizer Optimizer { get; }


        // ------ constructors ------ //

        public Affine(IOptimizer optimizer, int inshape, int outshape)
        {
            Optimizer = optimizer;
            var normal = new Normal(0, Math.Sqrt(1.0 / inshape));
            W = new DenseMatrix(inshape, outshape, Enumerable.Range(0, inshape * outshape).Select(_ => (float)normal.Sample()).ToArray());
            B = new DenseVector(outshape);
            GradW = new float[W.RowCount, W.ColumnCount];
            GradB = new float[B.Count];
        }

        public Affine(IOptimizer optimizer, DenseMatrix weights, DenseVector bias)
        {
            Optimizer = optimizer;
            W = weights;
            B = bias;
            GradW = new float[W.RowCount, W.ColumnCount];
            GradB = new float[B.Count];
        }


        // ------ public methods ------ //

        public Vector<float> Forward(Vector<float> x)
        {
            _x = x;
            return _x * W + B;
        }

        public Vector<float> Backward(Vector<float> dout)
        {
            for (int i = 0; i < _x.Count; i++)
                for (int j = 0; j < dout.Count; j++)
                    GradW[i, j] += _x[i] * dout[j];
            for (int i = 0; i < dout.Count; i++)
                GradB[i] += dout[i];
            return dout * W.Transpose();
        }

        public void Optimize()
        {
            var gw = DenseMatrix.OfArray(GradW);
            var gb = DenseVector.OfArray(GradB);
            (W, B) = Optimizer.Update(W, B, gw, gb);
            GradW = new float[W.RowCount, W.ColumnCount];
            GradB = new float[B.Count];
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

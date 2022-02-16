//using System.Linq;
//using System.Text.Json;
//using MathNet.Numerics.LinearAlgebra;
//using MathNet.Numerics.LinearAlgebra.Single;

//namespace Husty.NeuralNetwork
//{
//    // TODO: test
//    // see https://www.anarchive-beta.com/entry/2020/08/16/180000
//    public class BatchNormalization : ITunableLayer
//    {

//        // ------ fields ------ //

//        private Vector<float> _sigmas;
//        private Matrix<float> _xc;
//        private Matrix<float> _xn;


//        // ------ properties ------ //

//        public Matrix<float> W { private set; get; }

//        public Vector<float> B { private set; get; }

//        public Matrix<float> GradW { private set; get; }

//        public Vector<float> GradB { private set; get; }

//        public IOptimizer Optimizer { get; }


//        // ------ constructors ------ //

//        public BatchNormalization(IOptimizer optimizer, int size, float gamma = 1, float beta = 0)
//        {
//            Optimizer = optimizer;
//            W = DenseMatrix.OfRowArrays(Enumerable.Repeat(gamma, size).ToArray());
//            B = DenseVector.OfArray(Enumerable.Repeat(beta, size).ToArray());
//        }

//        public BatchNormalization(IOptimizer optimizer, DenseMatrix weights, DenseVector bias)
//        {
//            Optimizer = optimizer;
//            W = weights;
//            B = bias;
//        }


//        // ------ public methods ------ //

//        public Matrix<float> Forward(Matrix<float> x)
//        {
//            var means = x.ColumnSums() / x.RowCount;
//            _sigmas = (DenseMatrix.OfRowVectors(x.EnumerateRows().Select(r => (r - means).PointwisePower(2))).ColumnSums() / x.RowCount + 1e-7f).PointwiseSqrt();
//            _xc = DenseMatrix.OfRowVectors(x.EnumerateRows().Select(r => r - means));
//            _xn = DenseMatrix.OfRowVectors(_xc.EnumerateRows().Select(r => r / _sigmas));
//            return DenseMatrix.OfRowVectors(_xn.EnumerateRows().Select(r => r.PointwiseMultiply(W.Row(0)) + B));
//        }

//        public Matrix<float> Backward(Matrix<float> dout)
//        {
//            GradW = DenseMatrix.OfRowVectors(dout.PointwiseMultiply(_xc).ColumnSums());
//            GradB = dout.ColumnSums();
//            var dxn = DenseMatrix.OfRowVectors(dout.EnumerateRows().Select(r => r.PointwiseMultiply(W.Row(0))));
//            var dxc = DenseMatrix.OfRowVectors(dxn.EnumerateRows().Select(r => r / _sigmas));
//            var dsigmas = -DenseMatrix.OfRowVectors(dxn.PointwiseMultiply(_xc).EnumerateRows().Select(r => r / _sigmas.PointwisePower(2))).ColumnSums();
//            var dvar = 0.5f * dsigmas / _sigmas;
//            dxc += (2f / dout.RowCount) * DenseMatrix.OfRowVectors(_xc.EnumerateRows().Select(r => r.PointwiseMultiply(dvar)));
//            var dmean = dxc.ColumnSums() / dout.RowCount;
//            return DenseMatrix.OfRowVectors(dxc.EnumerateRows().Select(r => r / dmean));
//        }

//        public void Optimize()
//        {
//            (W, B) = Optimizer.Update(W, B, GradW, GradB);
//        }

//        public string Serialize()
//        {
//            var opt = JsonSerializer.Serialize<object>(Optimizer);
//            var w = JsonSerializer.Serialize(W.ToRowArrays());
//            var b = JsonSerializer.Serialize(B.ToArray());
//            return $"BatchNormalization::{opt}::{w}::{b}";
//        }

//        public static ILayer Deserialize(string[] line)
//        {
//            var opt = JsonSerializer.Deserialize<object>(line[0]) as IOptimizer;
//            var w = JsonSerializer.Deserialize<float[][]>(line[1]);
//            var b = JsonSerializer.Deserialize<float[]>(line[2]);
//            return new BatchNormalization(opt, DenseMatrix.OfRowArrays(w), DenseVector.OfArray(b));
//        }

//    }
//}

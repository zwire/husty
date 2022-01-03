using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public abstract class OptimizerBase : IOptimizer
    {

        public Matrix<double> Weight { private set; get; }

        public Vector<double> Bias { private set; get; }


        public (Matrix<double> W, Vector<double> B) Update(Matrix<double> w, Vector<double> b, Matrix<double> gw, Vector<double> gb)
        {
            Weight = Optimize(w, gw);
            Bias = Optimize(b, gb);
            return (Weight, Bias);
        }

        protected abstract Matrix<double> Optimize(Matrix<double> w, Matrix<double> gw);

        protected abstract Vector<double> Optimize(Vector<double> b, Vector<double> gb);

    }
}

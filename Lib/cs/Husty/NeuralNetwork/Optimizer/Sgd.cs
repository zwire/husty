using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public class Sgd : OptimizerBase
    {

        private readonly double _rate;

        public Sgd(double rate = 0.01)
        {
            _rate = rate;
        }

        protected override Matrix<double> Optimize(Matrix<double> w, Matrix<double> gw)
        {
            return w - _rate * gw;
        }

        protected override Vector<double> Optimize(Vector<double> b, Vector<double> gb)
        {
            return b - _rate * gb;
        }

    }
}

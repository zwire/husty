using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public class Sgd : OptimizerBase
    {

        // ------ fields ------ //

        private readonly float _rate;


        // ------ properties ------ //

        public float Rate => _rate;


        // ------ constructors ------ //

        public Sgd(float rate = 0.01f)
        {
            _rate = rate;
        }


        // ------ override methods ------ //

        protected override Matrix<float> Optimize(Matrix<float> w, Matrix<float> gw)
        {
            return w - _rate * gw;
        }

        protected override Vector<float> Optimize(Vector<float> b, Vector<float> gb)
        {
            return b - _rate * gb;
        }

        public override string Serialize()
        {
            return $"Sgd:<{JsonSerializer.Serialize(this)}";
        }

        internal static IOptimizer Deserialize(string line)
        {
            return JsonSerializer.Deserialize<Sgd>(line);
        }

    }
}

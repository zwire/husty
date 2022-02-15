using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork
{
    public sealed class NetworkGraph
    {


        // ------ properties ------ //

        public List<ILayer> LayerStack { get; }


        // ------ constructors ------ //

        public NetworkGraph(IEnumerable<ILayer> stack)
        {
            LayerStack = stack.ToList();
        }


        // ------ public methods ------ //

        public float[] Forward(float[] status)
        {
            return Forward(new[] { status })[0];
        }

        public float[][] Forward(float[][] statuses)
        {
            var mat = Matrix<float>.Build.DenseOfRowArrays(statuses);
            LayerStack.ForEach(n => mat = n.Forward(mat));
            return mat.ToRowArrays();
        }

        public float[] Backward(float[] error)
        {
            return Backward(new[] { error })[0];
        }

        public float[][] Backward(float[][] errors)
        {
            var mat = Matrix<float>.Build.DenseOfRowArrays(errors);
            for (int i = LayerStack.Count - 1; i > -1; i--)
                mat = LayerStack[i].Backward(mat);
            return mat.ToRowArrays();
        }

        public void Optimize()
        {
            LayerStack.OfType<ITunableLayer>().ToList().ForEach(l => l.Optimize());
        }

        public void Save(string name)
        {
            using var sw = new StreamWriter(name, false);
            LayerStack.ForEach(l => sw.WriteLine(l.Serialize()));
        }

        public static NetworkGraph Load(string name)
        {
            return new(File.ReadAllLines(name).Select(l => LayerFactory.Deserialize(l)));
        }

    }
}

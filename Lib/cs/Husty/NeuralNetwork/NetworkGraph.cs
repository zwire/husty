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

        public NetworkGraph(IEnumerable<ILayer> layers)
        {
            LayerStack = layers.ToList();
        }


        // ------ public methods ------ //

        public float[] Forward(float[] status)
        {
            var vec = Vector<float>.Build.DenseOfArray(status);
            LayerStack.ForEach(n => vec = n.Forward(vec));
            return vec.ToArray();
        }

        public void Backward(float[] error)
        {
            var vec = Vector<float>.Build.DenseOfArray(error);
            for (int i = LayerStack.Count - 1; i > -1; i--)
                vec = LayerStack[i].Backward(vec);
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

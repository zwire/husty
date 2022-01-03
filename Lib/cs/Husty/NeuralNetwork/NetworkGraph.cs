using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Husty.NeuralNetwork
{
    public class NetworkGraph : INetworkGraph
    {

        public List<ILayer> LayerStack { get; }

        public NetworkGraph(IEnumerable<ILayer> stack)
        {
            LayerStack = stack.ToList();
        }

        public double[] Forward(double[] status)
        {
            var vec = DenseVector.OfArray(status);
            LayerStack.ForEach(n => vec = n.Forward(vec));
            return vec.ToArray();
        }

        public void Backward(double[] error)
        {
            var vec = DenseVector.OfArray(error);
            for (int i = LayerStack.Count - 1; i > -1; i--)
            {
                vec = LayerStack[i].Backward(vec);
                if (LayerStack[i] is ITunableLayer l)
                {
                    var (w, b) = l.GetParams();
                    var (gw, gb) = l.GetGradients();
                    (w, b) = l.Optimizer.Update(w, b, gw, gb);
                    l.SetParams(w, b);
                }
            }
        }

        public void Backward(double error)
        {
            for (int i = LayerStack.Count - 1; i > 0; i--)
            {
                if (LayerStack[i] is ITunableLayer l)
                {
                    var outputShape = l.GetParams().B.Count;
                    Backward(Enumerable.Repeat(error, outputShape).ToArray());
                    break;
                }
            }
        }

        public void Save(string name)
        {
            using var sw = new StreamWriter(name, false);
            LayerStack.OfType<ITunableLayer>().ToList().ForEach(ts =>
            {
                var p = ts.GetParams();
                sw.WriteLine(JsonSerializer.Serialize(p.W.ToArray()));
                sw.WriteLine(JsonSerializer.Serialize(p.B.ToArray()));
            });
        }

        public void Load(string name)
        {
            using var sr = new StreamReader(name);
            LayerStack.OfType<ITunableLayer>().ToList().ForEach(ts =>
            {
                var w = DenseMatrix.OfArray(JsonSerializer.Deserialize<double[,]>(sr.ReadLine()));
                var b = DenseVector.OfArray(JsonSerializer.Deserialize<double[]>(sr.ReadLine()));
                ts.SetParams(w, b);
            });
        }

    }
}

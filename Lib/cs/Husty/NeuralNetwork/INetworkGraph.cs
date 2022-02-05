using System.Collections.Generic;

namespace Husty.NeuralNetwork
{
    public interface INetworkGraph
    {

        public List<ILayer> LayerStack { get; }

        public double[] Forward(double[] status);

        public void Backward(double[] error, bool freeze);

        public void Backward(double error, bool freeze);

        public void Save(string name);

        public void Load(string name);

    }
}

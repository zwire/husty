using System.Collections.Generic;

namespace Husty.NeuralNetwork
{
    public interface INetworkGraph
    {

        public List<ILayer> LayerStack { get; }

        public double[] Forward(double[] status);

        public void Backward(double[] error);

        public void Backward(double error);

        public void Save(string name);

        public void Load(string name);

    }
}

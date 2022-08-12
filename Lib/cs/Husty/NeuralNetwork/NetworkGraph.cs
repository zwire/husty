using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork;

public sealed class NetworkGraph : INeuralNetwork
{

    // ------ fields ------ //

    private readonly int _batchSize;
    private int _counter;


    // ------ properties ------ //

    public List<ILayer> LayerStack { get; }


    // ------ constructors ------ //

    public NetworkGraph(IEnumerable<ILayer> layers, int batchSize = 0)
    {
        LayerStack = layers.ToList();
        _batchSize = batchSize;
    }


    // ------ public methods ------ //

    public float[] Forward(float[] state)
    {
        var vec = Vector<float>.Build.DenseOfArray(state);
        LayerStack.ForEach(n => vec = n.Forward(vec));
        return vec.ToArray();
    }

    public void Backward(float[] error)
    {
        var vec = Vector<float>.Build.DenseOfArray(error);
        for (int i = LayerStack.Count - 1; i > -1; i--)
            vec = LayerStack[i].Backward(vec);
        if (_batchSize > 0 && ++_counter >= _batchSize)
        {
            LayerStack.OfType<ITunableLayer>().ToList().ForEach(l => l.Optimize());
            _counter = 0;
        }
    }

    public void Save(string name)
    {
        using var sw = new StreamWriter(name, false);
        LayerStack.ForEach(l => sw.WriteLine(l.Serialize()));
    }

    public static NetworkGraph Load(string name, int batchSize = 0)
    {
        return new(File.ReadAllLines(name).Select(l => LayerFactory.Deserialize(l)), batchSize);
    }

}

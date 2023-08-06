using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork;

public class Unit : ITunableLayer
{

  // ------ fields ------ //

  private readonly List<ILayer> _layerStack;


  // ------ constructors ------ //

  public Unit(IEnumerable<ILayer> layers)
  {
    _layerStack = layers.ToList();
  }


  // ------ public methods ------ //

  public Vector<float> Forward(Vector<float> x)
  {
    _layerStack.ForEach(n => x = n.Forward(x));
    return x;
  }

  public Vector<float> Backward(Vector<float> dout)
  {
    for (int i = _layerStack.Count - 1; i > -1; i--)
      dout = _layerStack[i].Backward(dout);
    return dout;
  }

  public void Optimize()
  {
    _layerStack.OfType<ITunableLayer>().ToList().ForEach(l => l.Optimize());
  }

  public string Serialize()
  {
    var txt = "Unit";
    _layerStack.ForEach(l => txt += $"<>{l.Serialize()}");
    return txt;
  }

  internal static ILayer Deserialize(string[] line)
  {
    return new Unit(line.Select(l => LayerFactory.Deserialize(l)).ToList());
  }

}

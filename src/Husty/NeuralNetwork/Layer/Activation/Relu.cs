using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Husty.NeuralNetwork;

public class Relu : IActivationLayer
{

  private Vector<float> _y;

  public Vector<float> Forward(Vector<float> x)
  {
    _y = x.Map(p => p > 0 ? p : 0);
    return _y;
  }

  public Vector<float> Backward(Vector<float> dout)
  {
    var dx = new DenseVector(dout.Count);
    for (int i = 0; i < dout.Count; i++)
      dx[i] = _y[i] > 0 ? dout[i] : 0;
    return dx;
  }

  public string Serialize()
  {
    return "Relu";
  }

  internal static ILayer Deserialize(string[] line)
  {
    return new Relu();
  }

}

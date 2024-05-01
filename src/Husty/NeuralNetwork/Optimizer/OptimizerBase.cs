using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork;

public abstract class OptimizerBase : IOptimizer
{

  public (Matrix<float> W, Vector<float> B) Update(Matrix<float> w, Vector<float> b, Matrix<float> gw, Vector<float> gb)
  {
    return (Optimize(w, gw), Optimize(b, gb));
  }

  protected abstract Matrix<float> Optimize(Matrix<float> w, Matrix<float> gw);

  protected abstract Vector<float> Optimize(Vector<float> b, Vector<float> gb);

  public abstract string Serialize();

}

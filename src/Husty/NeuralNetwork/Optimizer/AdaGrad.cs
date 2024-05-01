using System.Text.Json;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace Husty.NeuralNetwork;

public class AdaGrad : OptimizerBase
{

  // ------ fields ------ //

  private readonly float _rate;
  private Matrix<float> _hw;
  private Vector<float> _hb;


  // ------ properties ------ //

  public float Rate => _rate;


  // ------ constructors ------ //

  public AdaGrad(float rate = 0.01f)
  {
    _rate = rate;
  }


  // ------ override methods ------ //

  protected override Matrix<float> Optimize(Matrix<float> w, Matrix<float> gw)
  {
    if (_hw is null) _hw = new DenseMatrix(w.RowCount, w.ColumnCount);
    _hw += gw.PointwisePower(2);
    return w - _rate * gw.PointwiseDivide(_hw.PointwiseSqrt() + 1e-7f);
  }

  protected override Vector<float> Optimize(Vector<float> b, Vector<float> gb)
  {
    if (_hb is null) _hb = new DenseVector(b.Count);
    _hb += gb.PointwisePower(2);
    return b - _rate * gb / (_hb.PointwiseSqrt() + 1e-7f);
  }

  public override string Serialize()
  {
    return $"AdaGrad:<{JsonSerializer.Serialize(this)}";
  }

  internal static IOptimizer Deserialize(string line)
  {
    return JsonSerializer.Deserialize<AdaGrad>(line);
  }

}

using System.Text.Json;

namespace Husty.NeuralNetwork.Cmac;

public record struct CmacLabelInfo(int GridCount, float Lower, float Upper);

public sealed class CmacNetwork
{

  // ------ fields ------ //

  private readonly float _constraintMin;
  private readonly float _constraintMax;
  private readonly float _learningRate;
  private readonly CmacLabelInfo[] _infos;


  // ------ properties ------ //

  public CmacTable[] Tables { get; }


  // ------ constructors ------ //

  public CmacNetwork(
      IEnumerable<CmacLabelInfo> labelInfos,
      CmacTable[] tables,
      float constraintMin,
      float constraintMax,
      float learningRate
      )
  {
    _learningRate = learningRate;
    _constraintMin = constraintMin;
    _constraintMax = constraintMax;
    Tables = tables;
    _infos = labelInfos.ToArray();
    Forward(new float[labelInfos.Count()]);
  }

  public CmacNetwork(
      IEnumerable<CmacLabelInfo> labelInfos,
      int tableCount,
      float initialValue,
      float constraintMin,
      float constraintMax,
      float learningRate
  )
  {
    _learningRate = learningRate;
    _constraintMin = constraintMin;
    _constraintMax = constraintMax;
    Tables = new CmacTable[tableCount];
    _infos = labelInfos.ToArray();
    for (int i = 0; i < tableCount; i++)
    {
      var slidedInfos = new CmacLabelInfo[_infos.Length];
      for (int k = 0; k < _infos.Length; k++)
      {
        var step = (_infos[k].Upper - _infos[k].Lower) / _infos[k].GridCount;
        var lower = _infos[k].Lower + step * (i - tableCount / 2) / tableCount;
        var upper = _infos[k].Upper + step * (i - tableCount / 2) / tableCount;
        slidedInfos[k] = new(_infos[k].GridCount, lower, upper);
      }
      Tables[i] = new(slidedInfos, constraintMin / tableCount, constraintMax / tableCount, initialValue / tableCount);
    }
    Forward(new float[labelInfos.Count()]);
  }


  // ------ public methods ------ //

  public float Forward(float[] state)
  {
    if (state.Length != Tables[0].DimensionCount)
      throw new ArgumentException(nameof(state));
    var sum = 0f;
    foreach (var t in Tables)
    {
      t.FixLocation(state);
      sum += t.ActiveValue;
    }
    return sum;
  }

  public void Backward(float error, float[]? state = null)
  {
    var penalty = _learningRate * error / Tables.Length;
    foreach (var t in Tables)
    {
      if (state is not null)
        t.FixLocation(state);
      t.ApplyPenalty(penalty);
    }
  }

  public string Serialize()
  {
    var header0 = $"{Tables.Length},{_constraintMin},{_constraintMax},{_learningRate}";
    var header1 = JsonSerializer.Serialize(_infos);
    var str = header0 + "<>" + header1;
    foreach (var t in Tables)
      str += "<>" + JsonSerializer.Serialize(t.GetParams());
    return str;
  }

  public static CmacNetwork Deserialize(string str)
  {
    var lines = str.Split("<>");
    var header0 = lines[0].Split(",");
    var tableCount = int.Parse(header0[0]);
    var constraintMin = float.Parse(header0[1]);
    var constraintMax = float.Parse(header0[2]);
    var learningRate = float.Parse(header0[3]);
    var infos = JsonSerializer.Deserialize<CmacLabelInfo[]>(lines[1]);
    var net = new CmacNetwork(infos, tableCount, 0, constraintMin, constraintMax, learningRate);
    var bodies = lines[2..];
    for (int i = 0; i < bodies.Length; i++)
      net.Tables[i].SetParams(JsonSerializer.Deserialize<float[]>(bodies[i]));
    return net;
  }

  public CmacNetwork Clone()
  {
    return new(_infos, Tables.Select(x => x.Clone()).ToArray(), _constraintMin, _constraintMax, _learningRate);
  }

  public void Save(string name)
  {
    using var sw = new StreamWriter(name, false);
    sw.Write(Serialize());
  }

  public static CmacNetwork Load(string name)
  {
    return Deserialize(File.ReadAllText(name));
  }

}

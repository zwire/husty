namespace Husty.OpenCvSharp.Stats;

public record StatsVectorData(float[] Feature, bool Label);

public class DataContainer
{

  // ------ fields ------ //

  protected string _modelPath;
  protected List<StatsVectorData> _datas;


  // ------ constructors ------ //

  /// <summary>
  /// 
  /// </summary>
  /// <param name="dataPath">(.csv)</param>
  public DataContainer(string? dataPath = null)
  {
    _datas = new();
    if (dataPath is not null)
    {
      try
      {
        Load(dataPath);
      }
      catch
      {
        throw new FileLoadException("Invalid data format.");
      }
    }
  }


  // ------ public methods ------ //

  public StatsVectorData[] GetDataSet()
  {
    return _datas.ToArray();
  }

  /// <summary>
  /// Push back one vector data on dataset
  /// </summary>
  /// <param name="feature">Feature vector, such as HOG</param>
  /// <param name="label"></param>
  public void AddData(IEnumerable<float> feature, bool label)
  {
    _datas.Add(new(feature.ToArray(), label));
  }

  /// <summary>
  /// Remove n-1 index of data from dataset
  /// </summary>
  public void RemoveLastData()
  {
    if (_datas.Count is not 0)
      _datas.RemoveAt(_datas.Count - 1);
  }

  /// <summary>
  /// Remove all data from dataset
  /// </summary>
  public void ClearDataset()
  {
    _datas.Clear();
  }

  public void Load(string dataPath)
  {
    using var sr = new StreamReader(dataPath);
    while (sr.Peek() is not -1)
    {
      var strs = sr.ReadLine().Split(",");
      var feature = new List<float>();
      for (int i = 0; i < strs.Length - 1; i++)
        feature.Add(float.Parse(strs[i]));
      _datas.Add(new(feature.ToArray(), strs[strs.Length - 1] is "1"));
    }
  }

  public void Save(string path)
  {
    using var sw = new StreamWriter(path);
    _datas.ForEach(d =>
    {
      for (int i = 0; i < d.Feature.Length; i++)
        sw.Write($"{d.Feature[i]},");
      var label = d.Label ? 1 : 0;
      sw.Write($"{label}\n");
    });
  }

}

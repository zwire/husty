namespace Husty.OpenCvSharp.Stats;

public interface IBinaryStats : IDisposable
{

    public void Save(string modelPath);

    /// <summary>
    /// Train with current dataset.
    /// </summary>
    /// <param name="dataset"></param>
    public void Train(IEnumerable<StatsVectorData> dataset);

    /// <summary>
    /// </summary>
    /// <param name="input">Feature vectors vector</param>
    /// <returns>Sequence of 0 or 1</returns>
    public bool[] Predict(IEnumerable<IEnumerable<float>> input);

}

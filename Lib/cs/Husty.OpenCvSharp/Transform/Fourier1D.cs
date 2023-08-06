using OpenCvSharp;

namespace Husty.OpenCvSharp.Transform;

public sealed class Fourier1D
{

  // ------ fields ------ //

  private readonly double _samplingRate;
  private readonly int _sampleCount;
  private readonly Mat _complex;


  // ------ constructors ------ //

  /// <summary>
  /// 1D Fourier transformation for time series data.
  /// </summary>
  /// <param name="inputList">List of value</param>
  /// <param name="dt">Sampling time</param>
  public Fourier1D(IEnumerable<double> inputList, double dt)
  {
    _samplingRate = 1.0 / dt;
    _sampleCount = inputList.Count();
    using var real = new Mat(_sampleCount, 1, MatType.CV_32F, inputList.ToArray());
    _complex = new Mat();
    Cv2.Merge(new Mat[] { real, new Mat(_sampleCount, 1, MatType.CV_32F, 0.0) }, _complex);
  }


  // ------ public methods ------ //

  /// <summary>
  /// Discrete Fourier Transformation
  /// </summary>
  /// <returns>Histogram of frequency and value</returns>
  public unsafe (double Frequency, float Value)[] DftWithHistogram()
  {
    Cv2.Dft(_complex, _complex);
    Cv2.Split(_complex, out var planes);
    Cv2.Magnitude(planes[0], planes[1], planes[0]);
    var data = (float*)planes[0].Data;
    var result = new (double Frequency, float Value)[_sampleCount];
    for (int i = 0; i < _sampleCount / 2; i++)
      result[i] = ((double)i / _sampleCount * _samplingRate, data[i]);
    return result;
  }

  /// <summary>
  /// Discrete Fourier Transformation
  /// </summary>
  /// <returns>Feature of values</returns>
  public float[] Dft()
  {
    var list = DftWithHistogram();
    var feature = new float[list.Length];
    for (int i = 0; i < feature.Length; i++)
      feature[i] = list[i].Value;
    return feature;
  }

  /// <summary>
  /// Inverse Discrete Fourier Transformation
  /// </summary>
  /// <returns>Time and value</returns>
  public unsafe (double Time, float Value)[] IdftWithTime()
  {
    Cv2.Idft(_complex, _complex, DftFlags.Scale);
    Cv2.Split(_complex, out var planes);
    var data = (float*)planes[0].Data;
    var result = new (double Time, float Value)[_sampleCount];
    for (int i = 0; i < _sampleCount; i++)
      result[i] = (i / _samplingRate, data[i]);
    return result;
  }

  /// <summary>
  /// Inverse Discrete Fourier Transformation
  /// </summary>
  /// <returns>Feature of values</returns>
  public float[] Idft()
  {
    return IdftWithTime().Select(i => i.Value).ToArray();
  }

  /// <summary>
  /// Band-pass filter
  /// </summary>
  /// <param name="minFrequency"></param>
  /// <param name="maxFrequency"></param>
  public unsafe void Filter(double minFrequency, double maxFrequency)
  {
    for (int i = 0; i < _sampleCount; i++)
    {
      var data = (float*)_complex.Data;
      var freq = (double)i / _sampleCount * _samplingRate;
      if (freq < minFrequency || freq > maxFrequency)
      {
        data[i * 2 + 0] = 0;
        data[i * 2 + 1] = 0;
      }
    }
  }

}

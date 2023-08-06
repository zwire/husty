using System.Text.Json;
using System.Text.Json.Serialization;
using OpenCvSharp;

namespace Husty.OpenCvSharp.CameraCalibration;

internal class IntrinsicJson
{

  public JSize ImageSize { get; }

  public JMat CameraMatrix { get; }

  public JMat DistortionCoeffs { get; }

  [JsonConstructor]
  public IntrinsicJson(JSize imageSize, JMat cameraMatrix, JMat distortionCoeffs)
      => (ImageSize, CameraMatrix, DistortionCoeffs) = (imageSize, cameraMatrix, distortionCoeffs);

  internal IntrinsicJson(IntrinsicCameraParameters p)
  {
    ImageSize = p.ImageSize.ToJSize();
    CameraMatrix = p.CameraMatrix.ToJMat();
    DistortionCoeffs = p.DistortionCoeffs.ToJMat();
  }

  internal string Serialize()
  {
    return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
  }

  internal static IntrinsicCameraParameters Deserialize(string jstr)
  {
    var obj = JsonSerializer.Deserialize<IntrinsicJson>(jstr);
    return new(obj.ImageSize.ToSize(), obj.CameraMatrix.ToMat(), obj.DistortionCoeffs.ToMat());
  }

}

internal class ExtrinsicJson
{

  public JMat RotationMatrix { get; }

  public JMat TranslationVector { get; }

  [JsonConstructor]
  public ExtrinsicJson(JMat rotationMatrix, JMat translationVector)
      => (RotationMatrix, TranslationVector) = (rotationMatrix, translationVector);

  internal ExtrinsicJson(ExtrinsicCameraParameters p)
  {
    RotationMatrix = p.RotationMatrix.ToJMat();
    TranslationVector = p.TranslationVector.ToJMat();
  }

  internal string Serialize()
  {
    return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
  }

  internal static ExtrinsicCameraParameters Deserialize(string jstr)
  {
    var obj = JsonSerializer.Deserialize<ExtrinsicJson>(jstr);
    return new(obj.RotationMatrix.ToMat(), obj.TranslationVector.ToMat());
  }

}

internal record JSize(int Width, int Height) { }

internal record JMat(int Rows, int Cols, double[][] Data) { }


internal static class ConverterExtensions
{

  internal static JSize ToJSize(this Size size)
  {
    return new JSize(size.Width, size.Height);
  }

  internal static Size ToSize(this JSize jsize)
  {
    return new(jsize.Width, jsize.Height);
  }

  internal static JMat ToJMat(this Mat mat)
  {
    if (mat.Type() != MatType.CV_64F)
      throw new ArgumentException("Require: Type == CV_64F");
    var r = mat.Rows;
    var c = mat.Cols;
    var array = new double[r][];
    for (int y = 0; y < r; y++)
    {
      array[y] = new double[c];
      for (int x = 0; x < c; x++)
      {
        array[y][x] = mat.At<double>(y, x);
      }
    }
    return new JMat(r, c, array);
  }

  internal static Mat ToMat(this JMat jmat)
  {
    var r = jmat.Rows;
    var c = jmat.Cols;
    var array = jmat.Data;
    var mat = new Mat(r, c, MatType.CV_64F);
    for (int y = 0; y < r; y++)
    {
      for (int x = 0; x < c; x++)
      {
        mat.At<double>(y, x) = array[y][x];
      }
    }
    return mat;
  }

}


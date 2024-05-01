using System.IO.Compression;
using System.Text;
using OpenCvSharp;

namespace Husty.OpenCvSharp.ThreeDimensionalImaging;

public enum MatchingBase { Color, Depth }

public record struct BgrXyzPixel(byte B, byte G, byte R, short X, short Y, short Z)
{
  public static BgrXyzPixel Zero => new(0, 0, 0, 0, 0, 0);
  public void Deconstruct(out byte b, out byte g, out byte r, out short x, out short y, out short z)
  {
    b = B;
    g = G;
    r = R;
    x = X;
    y = Y;
    z = Z;
  }
}

public class BgrXyzImage : IDisposable
{

  // ------ properties ------ //

  public Mat Bgr { get; }

  public Mat X { get; }

  public Mat Y { get; }

  public Mat Z { get; }

  public int Width => Bgr.Width;

  public int Height => Bgr.Height;

  public int Rows => Bgr.Rows;

  public int Cols => Bgr.Cols;

  public bool IsDisposed => Bgr.IsDisposed || X.IsDisposed || Y.IsDisposed || Z.IsDisposed;

  public BgrXyzImage this[Rect box]
  {
    set
    {
      Bgr[box] = value.Bgr;
      X[box] = value.X;
      Y[box] = value.Y;
      Z[box] = value.Z;
    }
    get
    {
      return new(Bgr[box], X[box], Y[box], Z[box]);
    }
  }


  // ------ constructors ------ //

  public BgrXyzImage()
  {
    Bgr = new();
    X = new();
    Y = new();
    Z = new();
  }

  public BgrXyzImage(Mat bgr, Mat x, Mat y, Mat z)
  {
    Bgr = bgr;
    X = x;
    Y = y;
    Z = z;
    if (
        Bgr.Width != X.Width || Bgr.Height != X.Height ||
        X.Width != Y.Width || X.Height != Y.Height ||
        Y.Width != Z.Width || Y.Height != Z.Height
    )
      throw new InvalidOperationException("Require: Color size == XYZ size");
  }

  public BgrXyzImage(byte[] bgr, byte[] x, byte[] y, byte[] z)
  {
    Bgr = Cv2.ImDecode(bgr, ImreadModes.Unchanged);
    X = Cv2.ImDecode(x, ImreadModes.Unchanged);
    Y = Cv2.ImDecode(y, ImreadModes.Unchanged);
    Z = Cv2.ImDecode(z, ImreadModes.Unchanged);
    if (
        Bgr.Width != X.Width || Bgr.Height != X.Height ||
        X.Width != Y.Width || X.Height != Y.Height ||
        Y.Width != Z.Width || Y.Height != Z.Height
    )
      throw new InvalidOperationException("Require: Color size == XYZ size");
  }

  public unsafe BgrXyzImage(int width, int height, IEnumerable<BgrXyzPixel> src)
  {
    if (width * height != src.Count())
      throw new ArgumentException("Require: width * height == src.Count()");
    Bgr = new Mat(height, width, MatType.CV_8UC3);
    X = new Mat(height, width, MatType.CV_16UC1);
    Y = new Mat(height, width, MatType.CV_16UC1);
    Z = new Mat(height, width, MatType.CV_16UC1);
    var bgr = Bgr.DataPointer;
    var x = (short*)X.Data;
    var y = (short*)Y.Data;
    var z = (short*)Z.Data;
    var i = 0;
    foreach (var s in src)
    {
      bgr[i * 3 + 0] = s.B;
      bgr[i * 3 + 1] = s.G;
      bgr[i * 3 + 2] = s.R;
      x[i] = s.X;
      y[i] = s.Y;
      z[i] = s.Z;
      i++;
    }
  }


  // ------ public methods ------ //

  public void Dispose()
  {
    Bgr?.Dispose();
    X?.Dispose();
    Y?.Dispose();
    Z?.Dispose();
  }

  public void Deconstruct(out Mat bgr, out Mat x, out Mat y, out Mat z)
  {
    bgr = Bgr;
    x = X;
    y = Y;
    z = Z;
  }

  public BgrXyzImage Clone() => new(Bgr.Clone(), X.Clone(), Y.Clone(), Z.Clone());

  public bool Empty() => Bgr.Empty() || X.Empty() || Y.Empty() || Z.Empty();

  public void CopyFrom(Mat bgr, Mat x, Mat y, Mat z)
  {
    if (bgr.Width != Bgr.Width || bgr.Height != Bgr.Height ||
        x.Width != X.Width || x.Height != X.Height ||
        y.Width != Y.Width || y.Height != Y.Height ||
        z.Width != Z.Width || z.Height != Z.Height
    ) throw new InvalidOperationException("Require: src size == dst size");
    bgr.CopyTo(Bgr);
    x.CopyTo(X);
    y.CopyTo(Y);
    z.CopyTo(Z);
  }

  public BgrXyzImage Resize(Size size, InterpolationFlags flags = InterpolationFlags.Linear)
  {
    Cv2.Resize(Bgr, Bgr, size, 0, 0, flags);
    Cv2.Resize(X, X, size, 0, 0, flags);
    Cv2.Resize(Y, Y, size, 0, 0, flags);
    Cv2.Resize(Z, Z, size, 0, 0, flags);
    return this;
  }

  public BgrXyzImage Resize(double fx, double fy, InterpolationFlags flags = InterpolationFlags.Linear)
  {
    Cv2.Resize(Bgr, Bgr, new Size(), fx, fy, flags);
    Cv2.Resize(X, X, new Size(), fx, fy, flags);
    Cv2.Resize(Y, Y, new Size(), fx, fy, flags);
    Cv2.Resize(Z, Z, new Size(), fx, fy, flags);
    return this;
  }

  public unsafe Mat GetDepth16()
  {
    return Z.Clone();
  }

  public unsafe Mat GetDepth8(int minDistance, int maxDistance)
  {
    var d = GetDepth16();
    d.At<Vec3s>(0, 0) = new(0, 0, (short)maxDistance);
    Cv2.Threshold(d, d, maxDistance, maxDistance, ThresholdTypes.TozeroInv);
    Cv2.Threshold(d, d, minDistance, minDistance, ThresholdTypes.Tozero);
    Cv2.Normalize(d, d, 0, 255, NormTypes.MinMax, MatType.CV_8U);
    return d;
  }

  public unsafe BgrXyzPixel GetPixel(Point point)
  {
    if (point.X < 0 || point.Y < 0 || point.X >= Bgr.Width || point.Y >= Bgr.Height)
      throw new ArgumentOutOfRangeException();
    var index = point.Y * Bgr.Cols + point.X;
    var bgr = Bgr.DataPointer;
    var xp = (short*)X.Data;
    var yp = (short*)Y.Data;
    var zp = (short*)Z.Data;
    var b = bgr[index * 3 + 0];
    var g = bgr[index * 3 + 1];
    var r = bgr[index * 3 + 2];
    return new(b, g, r, xp[index], yp[index], zp[index]);
  }

  public unsafe void SetPixel(int x, int y, BgrXyzPixel value)
  {
    if (x < 0 || y < 0 || x >= Bgr.Width || y >= Bgr.Height)
      throw new ArgumentOutOfRangeException();
    var index = y * Bgr.Cols + x;
    var bgr = Bgr.DataPointer;
    var xp = (short*)X.Data;
    var yp = (short*)Y.Data;
    var zp = (short*)Z.Data;
    bgr[index * 3 + 0] = value.B;
    bgr[index * 3 + 1] = value.G;
    bgr[index * 3 + 2] = value.R;
    xp[index] = value.X;
    yp[index] = value.Y;
    zp[index] = value.Z;
  }

  public unsafe BgrXyzPixel[] ToArray(Func<BgrXyzPixel, bool> filter = null)
  {
    var bgr = Bgr.DataPointer;
    var x = (short*)X.Data;
    var y = (short*)Y.Data;
    var z = (short*)Z.Data;
    var ary = new BgrXyzPixel[Width * Height];
    for (int i = 0; i < Width * Height; i++)
    {
      var index = i * 3;
      var b = bgr[index + 0];
      var g = bgr[index + 1];
      var r = bgr[index + 2];
      ary[i] = new(b, g, r, x[i], y[i], z[i]);
    }
    if (filter is null)
      return ary;
    return ary.Where(filter).ToArray();
  }

  public unsafe BgrXyzImage Map(Func<BgrXyzPixel, BgrXyzPixel> func)
  {
    var bgr = Bgr.DataPointer;
    var x = (short*)X.Data;
    var y = (short*)Y.Data;
    var z = (short*)Z.Data;
    var ary = new BgrXyzPixel[Width * Height];
    for (int i = 0; i < Width * Height; i++)
    {
      var index = i * 3;
      var b = bgr[index + 0];
      var g = bgr[index + 1];
      var r = bgr[index + 2];
      ary[i] = func(new(b, g, r, x[i], y[i], z[i]));
    }
    return new(Width, Height, ary);
  }

  public unsafe BgrXyzImage Move(Vec3s delta)
  {
    var x = (short*)X.Data;
    var y = (short*)Y.Data;
    var z = (short*)Z.Data;
    for (int i = 0; i < Width * Height; i++)
    {
      x[i] += delta.Item0;
      y[i] += delta.Item1;
      z[i] += delta.Item2;
    }
    return this;
  }

  public unsafe BgrXyzImage Scale(Vec3s delta)
  {
    var x = (short*)X.Data;
    var y = (short*)Y.Data;
    var z = (short*)Z.Data;
    for (int i = 0; i < Width * Height; i++)
    {
      x[i] *= delta.Item0;
      y[i] *= delta.Item1;
      z[i] *= delta.Item2;
    }
    return this;
  }

  public unsafe BgrXyzImage Rotate(Mat rotationMat)
  {
    var d = (float*)rotationMat.Data;
    var xp = (short*)X.Data;
    var yp = (short*)Y.Data;
    var zp = (short*)Z.Data;
    for (int i = 0; i < Width * Height; i += 3)
    {
      var x = xp[i];
      var y = yp[i];
      var z = zp[i];
      xp[i] = (short)(d[0] * x + d[1] * y + d[2] * z);
      yp[i] = (short)(d[3] * x + d[4] * y + d[5] * z);
      zp[i] = (short)(d[6] * x + d[7] * y + d[8] * z);
    }
    return this;
  }

  public void SaveAsZip(string filePath)
  {
    if (File.Exists(filePath)) File.Delete(filePath);
    Cv2.ImWrite($"{filePath}_C.png", Bgr);
    Cv2.ImWrite($"{filePath}_X.png", X);
    Cv2.ImWrite($"{filePath}_Y.png", Y);
    Cv2.ImWrite($"{filePath}_Z.png", Z);
    using var z = ZipFile.Open($"{filePath}", ZipArchiveMode.Create);
    z.CreateEntryFromFile($"{filePath}_C.png", "C.png", CompressionLevel.Optimal);
    z.CreateEntryFromFile($"{filePath}_X.png", "X.png", CompressionLevel.Optimal);
    z.CreateEntryFromFile($"{filePath}_Y.png", "Y.png", CompressionLevel.Optimal);
    z.CreateEntryFromFile($"{filePath}_Z.png", "Z.png", CompressionLevel.Optimal);
    File.Delete($"{filePath}_C.png");
    File.Delete($"{filePath}_X.png");
    File.Delete($"{filePath}_Y.png");
    File.Delete($"{filePath}_Z.png");
  }

  public unsafe void SaveAsAsciiPly(string filePath)
  {
    var size = Width * Height;
    var cp = Bgr.DataPointer;
    var xp = (short*)X.Data;
    var yp = (short*)Y.Data;
    var zp = (short*)Z.Data;
    var lines = new List<string>();
    for (int i = 0; i < size; i++)
    {
      if (zp[i] > 0)
      {
        var b = cp[i * 3 + 0];
        var g = cp[i * 3 + 1];
        var r = cp[i * 3 + 2];
        var x = xp[i] * 0.001f;
        var y = yp[i] * 0.001f;
        var z = zp[i] * 0.001f;
        lines.Add($"{x} {y} {z} {r} {g} {b}");
      }
    }
    var headers = new[]
    {
            "ply",
            "format ascii 1.0",
            $"element vertex {lines.Count}",
            "property float x",
            "property float y",
            "property float z",
            "property uchar red",
            "property uchar green",
            "property uchar blue",
            "end_header"
        };
    using var writer = new StreamWriter(File.Open(filePath, FileMode.Create), Encoding.ASCII);
    foreach (var h in headers)
      writer.WriteLine(h);
    foreach (var line in lines)
      writer.WriteLine(line);
  }

  public unsafe void SaveAsBinaryPly(string filePath)
  {
    var size = Width * Height;
    var cp = Bgr.DataPointer;
    var xp = (short*)X.Data;
    var yp = (short*)Y.Data;
    var zp = (short*)Z.Data;
    var points = new List<byte[]>();
    for (int i = 0; i < size; i++)
    {
      if (zp[i] > 0)
      {
        var buf = new byte[15];
        Buffer.BlockCopy(new[] { xp[i] * 0.001f, yp[i] * 0.001f, zp[i] * 0.001f }, 0, buf, 0, 12);
        buf[12] = cp[i * 3 + 2];
        buf[13] = cp[i * 3 + 1];
        buf[14] = cp[i * 3 + 0];
        points.Add(buf);
      }
    }
    var headers = new[]
    {
            "ply",
            "format binary_little_endian 1.0",
            $"element vertex {points.Count}",
            "property float x",
            "property float y",
            "property float z",
            "property uchar red",
            "property uchar green",
            "property uchar blue",
            "end_header"
        };
    var stream = File.Open(filePath, FileMode.Create);
    var sw = new StreamWriter(stream, Encoding.ASCII);
    foreach (var h in headers)
      sw.WriteLine(h);
    sw.Close();
    stream = File.Open(filePath, FileMode.Append);
    var bw = new BinaryWriter(stream, Encoding.ASCII);
    bw.Seek(0, SeekOrigin.End);
    foreach (var pt in points)
      bw.Write(pt);
    bw.Close();
  }

  public static BgrXyzImage FromZip(string filePath)
  {
    if (!File.Exists(filePath))
      throw new FileNotFoundException();
    using var archive = ZipFile.OpenRead(filePath);
    archive.GetEntry("C.png").ExtractToFile("C.png", true);
    archive.GetEntry("X.png").ExtractToFile("X.png", true);
    archive.GetEntry("Y.png").ExtractToFile("Y.png", true);
    archive.GetEntry("Z.png").ExtractToFile("Z.png", true);
    var c = new Mat("C.png");
    var x = new Mat("X.png", ImreadModes.Unchanged);
    var y = new Mat("Y.png", ImreadModes.Unchanged);
    var z = new Mat("Z.png", ImreadModes.Unchanged);
    File.Delete("C.png");
    File.Delete("X.png");
    File.Delete("Y.png");
    File.Delete("Z.png");
    return new BgrXyzImage(c, x, y, z);
  }

}

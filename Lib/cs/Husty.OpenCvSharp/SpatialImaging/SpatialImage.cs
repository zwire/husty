using System.Text;
using System.IO.Compression;
using OpenCvSharp;

namespace Husty.OpenCvSharp.SpatialImaging;

public enum MatchingBase { Color, Depth }

public record struct SpatialImagePixel(byte R, byte G, byte B, short X, short Y, short Z)
{
    public static SpatialImagePixel Zero => new(0, 0, 0, 0, 0, 0);
}

public class SpatialImage : IDisposable
{

    // ------ properties ------ //

    public Mat Color { get; }

    public Mat X { get; }

    public Mat Y { get; }

    public Mat Z { get; }

    public int Width => Color.Width;

    public int Height => Color.Height;

    public int Rows => Color.Rows;

    public int Cols => Color.Cols;

    public bool IsDisposed => Color.IsDisposed || X.IsDisposed || Y.IsDisposed || Z.IsDisposed;

    public SpatialImage this[Rect box]
    {
        set
        {
            Color[box] = value.Color;
            X[box] = value.X;
            Y[box] = value.Y;
            Z[box] = value.Z;
        }
        get
        {
            return new(Color[box], X[box], Y[box], Z[box]);
        }
    }


    // ------ constructors ------ //

    public SpatialImage()
    {
        Color = new();
        X = new();
        Y = new();
        Z = new();
    }

    public SpatialImage(Mat color, Mat x, Mat y, Mat z)
    {
        Color = color;
        X = x;
        Y = y;
        Z = z;
        if (
            Color.Width != X.Width || Color.Height != X.Height ||
            X.Width != Y.Width || X.Height != Y.Height ||
            Y.Width != Z.Width || Y.Height != Z.Height
        )
            throw new InvalidOperationException("Require: Color size == XYZ size");
    }

    public SpatialImage(byte[] color, byte[] x, byte[] y, byte[] z)
    {
        Color = Cv2.ImDecode(color, ImreadModes.Unchanged);
        X = Cv2.ImDecode(x, ImreadModes.Unchanged);
        Y = Cv2.ImDecode(y, ImreadModes.Unchanged);
        Z = Cv2.ImDecode(z, ImreadModes.Unchanged);
        if (
            Color.Width != X.Width || Color.Height != X.Height ||
            X.Width != Y.Width || X.Height != Y.Height ||
            Y.Width != Z.Width || Y.Height != Z.Height
        )
            throw new InvalidOperationException("Require: Color size == XYZ size");
    }

    public unsafe SpatialImage(int width, int height, IEnumerable<SpatialImagePixel> src)
    {
        if (width * height != src.Count())
            throw new ArgumentException("Require: width * height == src.Count()");
        Color = new Mat(height, width, MatType.CV_8UC3);
        X = new Mat(height, width, MatType.CV_16UC1);
        Y = new Mat(height, width, MatType.CV_16UC1);
        Z = new Mat(height, width, MatType.CV_16UC1);
        var bgr = Color.DataPointer;
        var x = (short*)X.Data;
        var y = (short*)Y.Data;
        var z = (short*)Z.Data;
        var index = 0;
        foreach (var s in src)
        {
            var i = index * 3;
            bgr[i + 0] = s.B;
            bgr[i + 1] = s.G;
            bgr[i + 2] = s.R;
            x[i] = s.X;
            y[i] = s.Y;
            z[i] = s.Z;
            index++;
        }
    }


    // ------ public methods ------ //

    public void Dispose()
    {
        Color?.Dispose();
        X?.Dispose();
        Y?.Dispose();
        Z?.Dispose();
    }

    public void Deconstruct(out Mat color, out Mat x, out Mat y, out Mat z)
    {
        color = Color;
        x = X;
        y = Y;
        z = Z;
    }

    public SpatialImage Clone() => new(Color.Clone(), X.Clone(), Y.Clone(), Z.Clone());

    public bool Empty() => Color.Empty() || X.Empty() || Y.Empty() || Z.Empty();

    public void CopyFrom(Mat color, Mat x, Mat y, Mat z)
    {
        if (color.Width != Color.Width || color.Height != Color.Height ||
            x.Width != X.Width || x.Height != X.Height ||
            y.Width != Y.Width || y.Height != Y.Height ||
            z.Width != Z.Width || z.Height != Z.Height
        ) throw new InvalidOperationException("Require: src size == dst size");
        color.CopyTo(Color);
        x.CopyTo(X);
        y.CopyTo(Y);
        z.CopyTo(Z);
    }

    public SpatialImage Resize(Size size, InterpolationFlags flags = InterpolationFlags.Linear)
    {
        Cv2.Resize(Color, Color, size, 0, 0, flags);
        Cv2.Resize(X, X, size, 0, 0, flags);
        Cv2.Resize(Y, Y, size, 0, 0, flags);
        Cv2.Resize(Z, Z, size, 0, 0, flags);
        return this;
    }

    public SpatialImage Resize(double fx, double fy, InterpolationFlags flags = InterpolationFlags.Linear)
    {
        Cv2.Resize(Color, Color, Size.Zero, fx, fy, flags);
        Cv2.Resize(X, X, Size.Zero, fx, fy, flags);
        Cv2.Resize(Y, Y, Size.Zero, fx, fy, flags);
        Cv2.Resize(Z, Z, Size.Zero, fx, fy, flags);
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

    public unsafe SpatialImagePixel GetPixel(Point point)
    {
        if (point.X < 0 || point.Y < 0 || point.X >= Color.Width || point.Y >= Color.Height)
            throw new ArgumentOutOfRangeException();
        var index = point.Y * Color.Cols + point.X;
        var bgr = Color.DataPointer;
        var xp = (short*)X.Data;
        var yp = (short*)Y.Data;
        var zp = (short*)Z.Data;
        var b = bgr[index * 3 + 0];
        var g = bgr[index * 3 + 1];
        var r = bgr[index * 3 + 2];
        return new(b, g, r, xp[index], yp[index], zp[index]);
    }

    public unsafe void SetPixel(int x, int y, SpatialImagePixel value)
    {
        if (x < 0 || y < 0 || x >= Color.Width || y >= Color.Height)
            throw new ArgumentOutOfRangeException();
        var index = y * Color.Cols + x;
        var bgr = Color.DataPointer;
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

    public unsafe SpatialImagePixel[] ToArray(Func<SpatialImagePixel, bool> filter = null)
    {
        var bgr = Color.DataPointer;
        var x = (short*)X.Data;
        var y = (short*)Y.Data;
        var z = (short*)Z.Data;
        var ary = new SpatialImagePixel[Width * Height];
        for (int i = 0; i < Width * Height; i++)
        {
            var index = i * 3;
            var b = bgr[index + 0];
            var g = bgr[index + 1];
            var r = bgr[index + 2];
            ary[i] = new(b, g, r, x[index], y[index], z[index]);
        }
        if (filter is null)
            return ary;
        return ary.Where(filter).ToArray();
    }

    public unsafe SpatialImage Map(Func<SpatialImagePixel, SpatialImagePixel> func)
    {
        var bgr = Color.DataPointer;
        var x = (short*)X.Data;
        var y = (short*)Y.Data;
        var z = (short*)Z.Data;
        var ary = new SpatialImagePixel[Width * Height];
        for (int i = 0; i < Width * Height; i++)
        {
            var index = i * 3;
            var b = bgr[index + 0];
            var g = bgr[index + 1];
            var r = bgr[index + 2];
            ary[i] = func(new(b, g, r, x[index], y[index], z[index]));
        }
        return new(Width, Height, ary);
    }

    public unsafe SpatialImage Move(Vec3s delta)
    {
        var x = (short*)X.Data;
        var y = (short*)Y.Data;
        var z = (short*)Z.Data;
        for (int i = 0; i < X.Rows * X.Cols; i++)
        {
            x[i] += delta.Item0;
            y[i] += delta.Item1;
            z[i] += delta.Item2;
        }
        return this;
    }

    public unsafe SpatialImage Scale(Vec3s delta)
    {
        var x = (short*)X.Data;
        var y = (short*)Y.Data;
        var z = (short*)Z.Data;
        for (int i = 0; i < X.Rows * X.Cols; i++)
        {
            x[i] *= delta.Item0;
            y[i] *= delta.Item1;
            z[i] *= delta.Item2;
        }
        return this;
    }

    public unsafe SpatialImage Rotate(Mat rotationMat)
    {
        var d = (float*)rotationMat.Data;
        var xp = (short*)X.Data;
        var yp = (short*)Y.Data;
        var zp = (short*)Z.Data;
        for (int i = 0; i < X.Rows * X.Cols; i += 3)
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
        Cv2.ImWrite($"{filePath}_C.png", Color);
        Cv2.ImWrite($"{filePath}_X.png", X);
        Cv2.ImWrite($"{filePath}_Y.png", Y);
        Cv2.ImWrite($"{filePath}_Z.png", Z);
        using var z = ZipFile.Open($"{filePath}", ZipArchiveMode.Update);
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
        var cp = Color.DataPointer;
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
        var cp = Color.DataPointer;
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

    public static SpatialImage FromZip(string filePath)
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
        return new SpatialImage(c, x, y, z);
    }

}

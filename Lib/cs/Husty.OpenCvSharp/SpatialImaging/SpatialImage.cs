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

    public Mat ActualSpace { get; }

    public int Width => Color.Width;

    public int Height => Color.Height;

    public int Rows => Color.Rows;

    public int Cols => Color.Cols;

    public bool IsDisposed => Color.IsDisposed || ActualSpace.IsDisposed;

    public SpatialImage this[Rect box]
    {
        set
        {
            Color[box] = value.Color;
            ActualSpace[box] = value.ActualSpace;
        }
        get
        {
            return new(Color[box], ActualSpace[box]);
        }
    }


    // ------ constructors ------ //

    public SpatialImage()
    {
        Color = new();
        ActualSpace = new();
    }

    public SpatialImage(Mat color, Mat actualSpace)
    {
        Color = color;
        ActualSpace = actualSpace;
        if (Color.Width != ActualSpace.Width || Color.Height != ActualSpace.Height)
            throw new InvalidOperationException("Require: Color size == ActualSpace size");
    }

    public SpatialImage(byte[] color, byte[] actualSpace)
    {
        Color = Cv2.ImDecode(color, ImreadModes.Unchanged);
        ActualSpace = Cv2.ImDecode(actualSpace, ImreadModes.Unchanged);
        if (Color.Width != ActualSpace.Width || Color.Height != ActualSpace.Height)
            throw new InvalidOperationException("Require: Color size == ActualSpace size");
    }

    public unsafe SpatialImage(int width, int height, IEnumerable<SpatialImagePixel> src)
    {
        if (width * height != src.Count())
            throw new ArgumentException("Require: width * height == src.Count()");
        Color = new Mat(height, width, MatType.CV_8UC3);
        ActualSpace= new Mat(height, width, MatType.CV_16UC3);
        var bgr = Color.DataPointer;
        var xyz = (short*)ActualSpace.Data;
        var index = 0;
        foreach (var s in src)
        {
            var i = index * 3;
            bgr[i + 0] = s.B;
            bgr[i + 1] = s.G;
            bgr[i + 2] = s.R;
            xyz[i + 0] = s.X;
            xyz[i + 1] = s.Y;
            xyz[i + 2] = s.Z;
            index++;
        }
    }


    // ------ public methods ------ //

    public void Dispose()
    {
        Color?.Dispose();
        ActualSpace?.Dispose();
    }

    public void Deconstruct(out Mat color, out Mat actualSpace)
    {
        color = Color;
        actualSpace = ActualSpace;
    }

    public SpatialImage Clone() => new(Color.Clone(), ActualSpace.Clone());

    public bool Empty() => Color.Empty() || ActualSpace.Empty();

    public void CopyFrom(Mat color, Mat actualSpace)
    {
        if (color.Width != Color.Width || color.Height != Color.Height ||
            actualSpace.Width != ActualSpace.Width || actualSpace.Height != ActualSpace.Height
        ) throw new InvalidOperationException("Require: src size == dst size");
        color.CopyTo(Color);
        actualSpace.CopyTo(ActualSpace);
    }

    public SpatialImage Resize(Size size, InterpolationFlags flags = InterpolationFlags.Linear)
    {
        Cv2.Resize(Color, Color, size, 0, 0, flags);
        Cv2.Resize(ActualSpace, ActualSpace, size, 0, 0, flags);
        return this;
    }

    public SpatialImage Resize(double fx, double fy, InterpolationFlags flags = InterpolationFlags.Linear)
    {
        Cv2.Resize(Color, Color, Size.Zero, fx, fy, flags);
        Cv2.Resize(ActualSpace, ActualSpace, Size.Zero, fx, fy, flags);
        return this;
    }

    public unsafe Mat GetDepth16()
    {
        var d16 = new Mat();
        Cv2.ExtractChannel(ActualSpace, d16, 2);
        return d16;
    }

    public unsafe Mat GetDepth8(int minDistance, int maxDistance)
    {
        var d = GetDepth16();
        Cv2.Threshold(d, d, maxDistance, maxDistance, ThresholdTypes.TozeroInv);
        Cv2.Threshold(d, d, minDistance, minDistance, ThresholdTypes.Tozero);
        Cv2.Normalize(d, d, 0, 255, NormTypes.MinMax, MatType.CV_8U);
        return d;
    }

    public unsafe SpatialImagePixel GetPixel(Point point)
    {
        if (point.X < 0 || point.Y < 0 || point.X >= Color.Width || point.Y >= Color.Height)
            throw new ArgumentOutOfRangeException();
        var index = (point.Y * Color.Cols + point.X) * 3;
        var bgr = Color.DataPointer;
        var xyz = (short*)ActualSpace.Data;
        var b = bgr[index + 0];
        var g = bgr[index + 1];
        var r = bgr[index + 2];
        var x = xyz[index + 0];
        var y = xyz[index + 1];
        var z = xyz[index + 2];
        return new(b, g, r, x, y, z);
    }

    public unsafe SpatialImagePixel[] ToArray(Func<SpatialImagePixel, bool> filter = null)
    {
        var bgr = Color.DataPointer;
        var xyz = (short*)ActualSpace.Data;
        var ary = new SpatialImagePixel[Width * Height];
        for (int i = 0; i < Width * Height; i++)
        {
            var index = i * 3;
            var b = bgr[index + 0];
            var g = bgr[index + 1];
            var r = bgr[index + 2];
            var x = xyz[index + 0];
            var y = xyz[index + 1];
            var z = xyz[index + 2];
            ary[i] = new(b, g, r, x, y, z);
        }
        if (filter is null)
            return ary;
        return ary.Where(filter).ToArray();
    }

    public unsafe SpatialImage Map(Func<SpatialImagePixel, SpatialImagePixel> func)
    {
        var bgr = Color.DataPointer;
        var xyz = (short*)ActualSpace.Data;
        var ary = new SpatialImagePixel[Width * Height];
        for (int i = 0; i < Width * Height; i++)
        {
            var index = i * 3;
            var b = bgr[index + 0];
            var g = bgr[index + 1];
            var r = bgr[index + 2];
            var x = xyz[index + 0];
            var y = xyz[index + 1];
            var z = xyz[index + 2];
            ary[i] = func(new(b, g, r, x, y, z));
        }
        return new(Width, Height, ary);
    }

    public unsafe SpatialImage Move(Vec3s delta)
    {
        var s = (short*)ActualSpace.Data;
        var index = 0;
        for (int i = 0; i < ActualSpace.Rows * ActualSpace.Cols; i++)
        {
            s[index++] += delta.Item0;
            s[index++] += delta.Item1;
            s[index++] += delta.Item2;
        }
        return this;
    }

    public unsafe SpatialImage Scale(Vec3s delta)
    {
        var s = (short*)ActualSpace.Data;
        var index = 0;
        for (int i = 0; i < ActualSpace.Rows * ActualSpace.Cols; i++)
        {
            s[index++] *= delta.Item0;
            s[index++] *= delta.Item1;
            s[index++] *= delta.Item2;
        }
        return this;
    }

    public unsafe SpatialImage Rotate(Mat rotationMat)
    {
        var d = (float*)rotationMat.Data;
        var s = (short*)ActualSpace.Data;
        for (int i = 0; i < ActualSpace.Rows * ActualSpace.Cols * 3; i += 3)
        {
            var x = s[i + 0];
            var y = s[i + 1];
            var z = s[i + 2];
            s[i + 0] = (short)(d[0] * x + d[1] * y + d[2] * z);
            s[i + 1] = (short)(d[3] * x + d[4] * y + d[5] * z);
            s[i + 2] = (short)(d[6] * x + d[7] * y + d[8] * z);
        }
        return this;
    }

    public void SaveAsZip(string filePath)
    {
        Cv2.ImWrite($"{filePath}_C.png", Color);
        Cv2.ImWrite($"{filePath}_P.png", ActualSpace);
        using var z = ZipFile.Open($"{filePath}", ZipArchiveMode.Update);
        z.CreateEntryFromFile($"{filePath}_C.png", $"C.png", CompressionLevel.Optimal);
        z.CreateEntryFromFile($"{filePath}_P.png", $"P.png", CompressionLevel.Optimal);
        File.Delete($"{filePath}_C.png");
        File.Delete($"{filePath}_P.png");
    }

    public unsafe void SaveAsAsciiPly(string filePath)
    {
        var size = Width * Height;
        var cp = Color.DataPointer;
        var p = (short*)ActualSpace.Data;
        var lines = new List<string>();
        for (int i = 0; i < size; i++)
        {
            if (p[i * 3 + 2] is not 0)
            {
                var b = cp[i * 3 + 0];
                var g = cp[i * 3 + 1];
                var r = cp[i * 3 + 2];
                var x = p[i * 3 + 0] * 0.001f;
                var y = p[i * 3 + 1] * 0.001f;
                var z = p[i * 3 + 2] * 0.001f;
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
        var p = (short*)ActualSpace.Data;
        var points = new List<byte[]>();
        for (int i = 0; i < size; i++)
        {
            if (p[i * 3 + 2] is not 0)
            {
                var buf = new byte[15];
                Buffer.BlockCopy(new[] { p[i * 3 + 0] * 0.001f, p[i * 3 + 1] * 0.001f, p[i * 3 + 2] * 0.001f }, 0, buf, 0, 12);
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
        archive.GetEntry("P.png").ExtractToFile("P.png", true);
        var c = new Mat("C.png");
        var p = new Mat("P.png", ImreadModes.Unchanged);
        File.Delete("C.png");
        File.Delete("P.png");
        return new(c, p);
    }

}

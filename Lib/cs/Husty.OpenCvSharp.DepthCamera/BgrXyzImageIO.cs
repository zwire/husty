using System.IO.Compression;
using System.Text;
using OpenCvSharp;

namespace Husty.OpenCvSharp.DepthCamera;

/// <summary>
/// Saving & Loading for images captured by depth cameras
/// </summary>
public static class BgrXyzImageIO
{

    // ------ public methods ------ //

    /// <summary>
    /// Save RGB, Depth, and PointCloud image in zip file.
    /// </summary>
    /// <param name="directory">Target directory</param>
    /// <param name="name">To identify</param>
    public static void SaveAsZip(string directory, string name, BgrXyzMat image)
    {
        directory ??= Directory.GetCurrentDirectory();
        name ??= "_";
        var filePath = $"{directory}\\Image{name}.zip";
        Cv2.ImWrite($"{filePath}_C.png", image.BGR);
        Cv2.ImWrite($"{filePath}_D.png", image.Depth16());
        Cv2.ImWrite($"{filePath}_P.png", image.XYZ);
        using var z = ZipFile.Open($"{filePath}", ZipArchiveMode.Update);
        z.CreateEntryFromFile($"{filePath}_C.png", $"C.png", CompressionLevel.Optimal);
        z.CreateEntryFromFile($"{filePath}_D.png", $"D.png", CompressionLevel.Optimal);
        z.CreateEntryFromFile($"{filePath}_P.png", $"P.png", CompressionLevel.Optimal);
        File.Delete($"{filePath}_C.png");
        File.Delete($"{filePath}_D.png");
        File.Delete($"{filePath}_P.png");
    }

    public static unsafe void SaveAsAsciiPly(string directory, string name, BgrXyzMat image)
    {
        var size = image.Width * image.Height;
        var cp = image.BGR.DataPointer;
        var p = (short*)image.XYZ.Data;
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
        directory ??= Directory.GetCurrentDirectory();
        name ??= "_";
        var filePath = $"{directory}\\{name}.ply";
        using var writer = new StreamWriter(File.Open(filePath, FileMode.Create), Encoding.ASCII);
        foreach (var h in headers)
            writer.WriteLine(h);
        foreach (var line in lines)
            writer.WriteLine(line);
    }

    public static unsafe void SaveAsBinaryPly(string directory, string name, BgrXyzMat image)
    {
        var size = image.Width * image.Height;
        var cp = image.BGR.DataPointer;
        var p = (short*)image.XYZ.Data;
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
        directory ??= Directory.GetCurrentDirectory();
        name ??= "_";
        var filePath = $"{directory}\\{name}.ply";
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

    /// <summary>
    /// Extract images from zip file.
    /// </summary>
    /// <param name="filePath">Zip file path</param>
    /// <returns></returns>
    public static BgrXyzMat OpenZip(string filePath)
    {
        if (!File.Exists(filePath)) throw new Exception("File doesn't Exist!");
        using var archive = ZipFile.OpenRead(filePath);
        archive.GetEntry("C.png").ExtractToFile("C.png", true);
        archive.GetEntry("P.png").ExtractToFile("P.png", true);
        var c = new Mat("C.png");
        var p = new Mat("P.png", ImreadModes.Unchanged);
        File.Delete("C.png");
        File.Delete("P.png");
        return new BgrXyzMat(c, p);
    }

}

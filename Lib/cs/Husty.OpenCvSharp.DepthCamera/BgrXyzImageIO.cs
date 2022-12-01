using System;
using System.IO.Compression;
using System.Text;
using Intel.RealSense.Math;
using Intel.RealSense;
using MathNet.Numerics;
using System.Xml.Linq;
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

    public static void SaveAsPly(string directory, string name, BgrXyzMat image, Func<BGRXYZ, bool> filter = null)
    {
        var pts = image.ToArray(filter);
        var headers = new[]
        {
            "ply",
            "format ascii 1.0",
            $"element vertex {pts.Length}",
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
        foreach (var p in pts)
        {
            writer.WriteLine($"{p.X * 0.001f} {p.Y * 0.001f} {p.Z * 0.001f} {p.R} {p.G} {p.B}");
        }
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
        return new BgrXyzMat(new Mat("C.png"), new Mat("P.png", ImreadModes.Unchanged));
    }

}

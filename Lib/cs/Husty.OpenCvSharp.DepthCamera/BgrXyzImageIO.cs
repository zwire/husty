using System.IO.Compression;
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
    /// <param name="saveDirectory">Target directory</param>
    /// <param name="id">To identify</param>
    public static void SaveAsZip(string saveDirectory, string id, BgrXyzMat input)
    {
        id = id is null ? "" : "_" + id;
        var filePath = $"{saveDirectory}\\Image{id}.zip";
        Cv2.ImWrite($"{filePath}_C.png", input.BGR);
        Cv2.ImWrite($"{filePath}_D.png", input.Depth16());
        Cv2.ImWrite($"{filePath}_P.png", input.XYZ);
        using var z = ZipFile.Open($"{filePath}", ZipArchiveMode.Update);
        z.CreateEntryFromFile($"{filePath}_C.png", $"C.png", CompressionLevel.Optimal);
        z.CreateEntryFromFile($"{filePath}_D.png", $"D.png", CompressionLevel.Optimal);
        z.CreateEntryFromFile($"{filePath}_P.png", $"P.png", CompressionLevel.Optimal);
        File.Delete($"{filePath}_C.png");
        File.Delete($"{filePath}_D.png");
        File.Delete($"{filePath}_P.png");
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

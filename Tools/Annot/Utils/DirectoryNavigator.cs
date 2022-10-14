using System.IO;
using System.Linq;
using OpenCvSharp;

namespace Annot.Utils;

public class DirectoryNavigator
{

    // ------ fields ------ //

    private int _index;
    private readonly string[] _imagePaths;
    private readonly string[] _labelPaths;


    // ------ properties ------ //

    public bool IsFirst => _index is 0;

    public bool IsLast => _index == _imagePaths.Length - 1;

    public (string ImagePath, string LabelPath) Current => (_imagePaths[_index], _labelPaths[_index]);

    public string[] ImagePaths => _imagePaths;

    public string[] LabelPaths => _labelPaths;


    // ------ constructors ------ //

    public DirectoryNavigator(string inputDir, string outputDir)
    {
        _imagePaths = Directory.GetFiles(inputDir)
            .Where(f =>
            {
                try
                {
                    Cv2.ImRead(f).Dispose();
                    return true;
                }
                catch
                {
                    return false;
                }
            })
            .ToArray();
        _labelPaths = _imagePaths
            .Select(f => Path.Combine(outputDir, Path.GetFileNameWithoutExtension(f) + ".json"))
            .ToArray();
        foreach (var f in _labelPaths)
            if (!File.Exists(f))
                File.WriteAllText(f, "");
    }


    // ------ public methods ------ //

    public (string ImagePath, string LabelPath) Move(int index)
    {
        _index = index;
        return (_imagePaths[_index], _labelPaths[_index]);
    }

}

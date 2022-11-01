using System.IO;
using System.Linq;
using OpenCvSharp;

namespace Annot.Utils;

public class DirectoryNavigator
{

    // ------ fields ------ //

    private int _index;
    private readonly string[] _imagePaths;


    // ------ properties ------ //

    public bool IsFirst => _index is 0;

    public bool IsLast => _index == _imagePaths.Length - 1;

    public string Current => _imagePaths[_index];

    public string[] ImagePaths => _imagePaths;


    // ------ constructors ------ //

    public DirectoryNavigator(string inputDir)
    {
        _imagePaths = Directory.GetFiles(inputDir)
            .Where(f =>
            {
                try
                {
                    using var img = Cv2.ImRead(f);
                    return !img.Empty();
                }
                catch
                {
                    return false;
                }
            })
            .ToArray();
    }


    // ------ public methods ------ //

    public string Move(int index)
    {
        _index = index;
        return _imagePaths[_index];
    }

}

using OpenCvSharp;

namespace Husty.OpenCvSharp.HyperSpectralImaging;

public class NHImage : HyperSpectralImageBase
{

    // ------ constructors ------ //

    public NHImage(string filePath, Size size, int bands, Func<int, int> convertWaveLengthToChannel)
        : base(Load(filePath, size, bands), convertWaveLengthToChannel) { }

    public NHImage(string filePath)
            : base(Load(filePath, default, default),
                wl => Path.GetExtension(filePath) switch
                {
                    ".nh1" => (wl - 400) / 5,
                    ".nh3" => (wl - 350) / 5,
                    ".nh5" => (wl - 350) / 5,
                    ".nh7" => (wl - 350) / 5,
                    ".nh9" => (wl - 350) / 5,
                    _ => throw new NotImplementedException()
                })
    { }


    // ------ private methods ------ //

    private unsafe static Mat Load(string filePath, Size size, int bands)
    {
        if (!File.Exists(filePath))
            throw new ArgumentException("file doesn't exist!");
        switch (Path.GetExtension(filePath))
        {
            case ".nh1":
                size = new(640, 480);
                bands = 141;
                break;
            case ".nh3":
                size = new(752, 480);
                bands = 151;
                break;
            case ".nh5":
                size = new(1024, 768);
                bands = 151;
                break;
            case ".nh7":
                size = new(1280, 1024);
                bands = 151;
                break;
            case ".nh9":
                size = new(2048, 1080);
                bands = 151;
                break;
            default:
                break;
        }
        var span = File.ReadAllBytes(filePath);
        var image = new Mat(size, MatType.CV_64FC(bands));
        var ptr = (double*)image.Data;
        var w = image.Width;
        var h = image.Height;
        Parallel.For(0, h, y =>
        {
            var perY = y * w * bands;
            for (int x = 0; x < w; x++)
            {
                var perX = x * bands;
                for (int c = 0; c < bands; c++)
                {
                    var loc = 2 * (perY + x + c * w);
                    ptr[perY + perX + c] = span[loc] | span[loc + 1] << 8;
                }
            }
        });
        return image;
    }

}
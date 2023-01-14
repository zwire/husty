using Husty.Extensions;
using Husty.OpenCvSharp.Extensions;
using OpenCvSharp;

namespace Husty.OpenCvSharp.HyperSpectralImaging;

public abstract class HyperSpectralImageBase : IDisposable
{

    // ------ fields ------ //

    private readonly Mat _image;
    private readonly Func<int, int> _convert;


    // ------ properties ------ //

    public Mat RawImage => _image;

    public int Width => _image.Width;

    public int Height => _image.Height;


    // ------ constructors ------ //

    public unsafe HyperSpectralImageBase(Mat image, Func<int, int> convertWaveLengthToChannel)
    {
        _image = image;
        _convert = convertWaveLengthToChannel;
    }


    // ------ public methods ------ //

    public void Dispose()
    {
        _image.Dispose();
    }

    public Mat GetImage(int waveLength)
    {
        return _image.ExtractChannel(_convert(waveLength));
    }

    public Mat[] GetImages(params int[] waveLengths)
    {
        return waveLengths.Select(w => GetImage(w)).ToArray();
    }

    public Mat GetViewImage(int waveLength)
    {
        var img = GetImage(_convert(waveLength));
        Cv2.Normalize(img, img, 255, 0, NormTypes.MinMax);
        img.ConvertTo(img, MatType.CV_8UC1);
        return img;
    }

    public Mat[] GetViewImages(params int[] waveLengths)
    {
        return waveLengths.Select(w => GetViewImage(w)).ToArray();
    }

    public double[,] Get2DArray(int waveLength)
    {
        return GetImage(_convert(waveLength)).To1DDoubleArray().To2DArray(Width, Height);
    }

    public double[][] Get2DJaggedArray(int waveLength)
    {
        return GetImage(_convert(waveLength)).To1DDoubleArray().To2DJaggedArray(Width, Height);
    }

}

using OpenCvSharp;

namespace Husty.OpenCvSharp.Feature;

/// <summary>
/// OpenCvSharp 'HOGDescriptor' class wrapper.
/// </summary>
public sealed class Hog
{

    // ------ fields ------ //

    private readonly HOGDescriptor _hog;
    private readonly Size _imageSize;


    // ------ constructors ------ //

    /// <summary>
    /// Simple HOG descriptor
    /// </summary>
    /// <param name="imageSize"></param>
    /// <param name="blockSize">For normalization</param>
    /// <param name="blockStride"></param>
    /// <param name="cellSize">Unit size for computation</param>
    public Hog(Size? imageSize = null, Size? blockSize = null, Size? blockStride = null, Size? cellSize = null)
    {
        var s1 = imageSize ?? new Size(64, 64);
        var s2 = blockSize ?? new Size(16, 16);
        var s3 = blockStride ?? new Size(8, 8);
        var s4 = cellSize ?? new Size(8, 8);
        _hog = new HOGDescriptor(s1, s2, s3, s4);
        _imageSize = s1;
    }


    // ------ public methods ------ //

    /// <summary>
    /// Process one frame.
    /// </summary>
    /// <param name="input">8 bit gray-scale image. It's going to be resized automatically.</param>
    /// <returns>Feature of values</returns>
    public float[] Compute(Mat input)
    {
        if (input.Type() != MatType.CV_8U) new Exception("MatType should be 'CV_8U'.");
        Cv2.Resize(input, input, _imageSize);
        return _hog.Compute(input);
    }

}

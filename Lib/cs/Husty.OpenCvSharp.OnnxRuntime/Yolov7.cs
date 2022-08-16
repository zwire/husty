using OpenCvSharp;
using Husty.Extensions;

namespace Husty.OpenCvSharp.Yolo;

public sealed class Yolov7 : OnnxBase<Mat, YoloResult[]>, IYoloDetector
{

    // ------ fields ------ //

    private readonly Size _size;
    private readonly Mat _paddingBase;
    private readonly float _confThresh;
    private readonly string[] _labels;


    // ------ constructors ------ //

    public Yolov7(
        string onnxModel,
        string namesFile,
        float confThresh = 0.3f,
        Provider provider = Provider.CPU,
        OptimizationLevel opt = OptimizationLevel.Off
    ) : base(onnxModel, provider, opt)
    {
        _size = new(InputLayers[0].Shape[2], InputLayers[0].Shape[3]);
        _paddingBase = new(_size.Height, _size.Width, MatType.CV_8UC3, new(114, 114, 114));
        _confThresh = confThresh;
        _labels = File.ReadAllLines(namesFile).Where(x => x is not null && x is not "").ToArray();
    }


    // ------ public methods ------ //

    public unsafe override YoloResult[] Run(Mat frame)
    {
        // pre-process --- resizing
        var wr = (double)_size.Width / frame.Width;
        var hr = (double)_size.Height / frame.Height;
        var ratio = Math.Min(wr, hr);
        var wRatio = ratio == wr ? 1.0 : wr / hr;
        var hRatio = ratio == hr ? 1.0 : hr / wr;
        var padded = _paddingBase.Clone();
        var size = new Size(frame.Width * ratio, frame.Height * ratio);
        padded[new Rect(0, 0, size.Width, size.Height)] = frame.Resize(size);
        padded.ConvertTo(padded, MatType.CV_32FC3);
        padded /= 255f;

        // input array into model
        var d = (float*)padded.Data;
        var len = padded.Rows * padded.Cols;
        var inputArray = new float[len * 3];
        for (int i = 0; i < len; i++)
        {
            inputArray[i + len * 0] = d[i * 3 + 2];
            inputArray[i + len * 1] = d[i * 3 + 1];
            inputArray[i + len * 2] = d[i * 3 + 0];
        }
        padded.Dispose();
        var o = base.Run(inputArray);
        if (!o.ContainsKey("output"))
            return Array.Empty<YoloResult>();
        var outs = o["output"];
        if (outs.Length is 0) 
            return Array.Empty<YoloResult>();
        return outs
            .To2DJaggedArray(outs.Length / 7, 7)
            .Where(r => r[6] >= _confThresh)
            .Select(r =>
            {
                var w = (r[3] - r[1]) / (ratio * frame.Width);
                var h = (r[4] - r[2]) / (ratio * frame.Height);
                var x = (r[1] + r[3]) / (ratio * frame.Width) / 2 - w / 2;
                var y = (r[2] + r[4]) / (ratio * frame.Height) / 2 - h / 2;
                var id = (int)r[5];
                var score = r[6];
                return new YoloResult(new Rect2d(x, y, w, h), score, _labels[id], score);
            }).ToArray();
    }

}

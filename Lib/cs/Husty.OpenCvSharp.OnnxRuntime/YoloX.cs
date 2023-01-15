using OpenCvSharp;
using OpenCvSharp.Dnn;
using Husty.Extensions;

namespace Husty.OpenCvSharp.Yolo;

public sealed class YoloX : OnnxBase<Mat, YoloResult[]>, IYoloDetector
{

    // ------ fields ------ //

    private readonly int[] _strides = new[] { 8, 16, 32 };
    private readonly Size _size;
    private readonly int _detectionCount;
    private readonly float _confThresh;
    private readonly string[] _labels;
    private readonly Mat _paddingBase;


    // ------ constructors ------ //

    /// <summary>
    /// Initialize detector (YOLOX ONNX format)
    /// </summary>
    /// <param name="onnxModel">(.onnx) file</param>
    /// <param name="namesFile">(.names) file</param>
    /// <param name="confidenceThreshold"></param>
    public YoloX(
        string onnxModel, 
        string namesFile, 
        float confidenceThreshold = 0.5f,
        Provider provider = Provider.CPU,
        OptimizationLevel opt = OptimizationLevel.Off
    ) : base(onnxModel, provider, opt)
    {
        _size = new(InputLayers[0].Shape[3], InputLayers[0].Shape[2]);
        _confThresh = confidenceThreshold;
        _detectionCount = _strides.Select(x => _size.Width / x * _size.Height / x).Sum();
        _paddingBase = new(_size.Height, _size.Width, MatType.CV_8UC3, new Scalar(114, 114, 114));
        _labels = File.ReadAllLines(namesFile);
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
        using var padded = _paddingBase.Clone();
        var size = new Size(frame.Width * ratio, frame.Height * ratio);
        padded[new Rect(0, 0, size.Width, size.Height)] = frame.Resize(size);

        // input array into model
        var d = padded.DataPointer;
        var len = padded.Rows * padded.Cols;
        var inputArray = new float[len * 3];
        for (int i = 0; i < len; i++)
        {
            inputArray[i + len * 0] = d[i * 3 + 2];
            inputArray[i + len * 1] = d[i * 3 + 1];
            inputArray[i + len * 2] = d[i * 3 + 0];
        }
        var o = base.Run(inputArray);
        if (!o.ContainsKey("output"))
            return Array.Empty<YoloResult>();
        var outs = o["output"].To2DJaggedArray(_detectionCount, _labels.Length + 5);

        // post-process --- get coordinate from output
        var ids = new List<int>();
        var confs = new List<float>();
        var probs = new List<float>();
        var boxes = new List<Rect2d>();
        var baseIndex = 0;
        foreach (var stride in _strides)
        {
            var xGrids = padded.Width / stride;
            var yGrids = padded.Height / stride;
            for (int i = 0; i < yGrids; i++)
            {
                for (int j = 0; j < xGrids; j++)
                {
                    var p = outs[baseIndex + i * xGrids + j];
                    var index = p.Skip(5).ToArray().ArgMax(out var prob);
                    var conf = p[4];
                    if (conf * prob > _confThresh)
                    {
                        var x = (p[0] + j) / xGrids * wRatio;
                        var y = (p[1] + i) / yGrids * hRatio;
                        var w = Math.Exp(p[2]) / xGrids * wRatio;
                        var h = Math.Exp(p[3]) / yGrids * hRatio;
                        boxes.Add(new(x - w / 2, y - h / 2, w, h));
                        ids.Add(index);
                        confs.Add(conf);
                        probs.Add((float)prob);
                    }
                }
            }
            baseIndex += xGrids * yGrids;
        }
        CvDnn.NMSBoxes(boxes, confs, _confThresh, 0.3f, out var indices);
        return indices.Select(i => new YoloResult(boxes[i], confs[i], _labels[ids[i]], probs[i])).ToArray();
    }

}

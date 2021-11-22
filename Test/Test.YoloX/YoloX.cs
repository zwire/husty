using System;
using System.Collections.Generic;
using System.Linq;
using Husty;
using Husty.OnnxRuntime;
using Husty.OpenCvSharp;
using OpenCvSharp;

namespace Test.YoloX
{

    public record YoloXResult(Rect Box, double Probability, int Label);

    public class YoloX : OnnxBase<Mat, IEnumerable<YoloXResult>>
    {

        private const int _classCount = 80;
        private readonly int[] _strides = new[] { 8, 16, 32 };
        private readonly Size _size;
        private readonly int _detectionCount;
        private readonly double _confThresh;
        private readonly Mat _paddingBase;

        public YoloX(string model, double confidenceThreshold = 0.5) 
            : base(model, Provider.CPU, OptimizationLevel.Off) 
        {
            if (model.Contains("tiny") || model.Contains("nano"))
                _size = new(416, 416);
            else
                _size = new(640, 640);
            _confThresh = confidenceThreshold;
            _detectionCount = _strides.Select(x => _size.Width / x * _size.Height / x).Sum();
            _paddingBase = new(_size.Height, _size.Width, MatType.CV_8UC3, new(114, 114, 114));
        }


        public override IEnumerable<YoloXResult> Run(Mat frame)
        {
            var ratio = 1.0 / Math.Min((double)_size.Width / frame.Width, (double)_size.Height / frame.Height);
            var padded = _paddingBase.Clone();
            var size = new Size(frame.Width / ratio, frame.Height / ratio);
            padded[new Rect(0, 0, size.Width, size.Height)] = frame.Resize(size);
            var inputArray = ToInputArray(padded);
            var outputArray = base.Run(inputArray)["output"].To2DJaggedArray(_detectionCount, _classCount + 5);

            foreach (var stride in new[] { 8, 16, 32 })
            {
                var scale = stride * ratio;
                var xGrids = padded.Width / stride;
                var yGrids = padded.Height / stride;
                for (int i = 0; i < yGrids; i++)
                {
                    for (int j = 0; j < xGrids; j++)
                    {
                        var r = outputArray[i * xGrids + j];
                        var index = r.Skip(5).ToArray().ArgMax(out var prob);
                        if (r[4] > _confThresh)
                        {
                            var x = r[0] + j;
                            var y = r[1] + i;
                            var w = Math.Exp(r[2]);
                            var h = Math.Exp(r[3]);
                            var box = new Rect2d(x - w / 2, y - h / 2, w, h).Scale(scale, scale).ToRect();
                            yield return new(box, prob, index);
                        }
                    }
                }
            }

        }

        private unsafe float[] ToInputArray(Mat frame)
        {
            var frame2 = frame.CvtColor(ColorConversionCodes.BGR2RGB);
            var d = frame2.DataPointer;
            var len = frame2.Rows * frame2.Cols;
            var array = new float[len * 3];
            for (int i = 0; i < len; i++)
            {
                array[i + len * 0] = d[i * 3 + 0];
                array[i + len * 1] = d[i * 3 + 1];
                array[i + len * 2] = d[i * 3 + 2];
            }
            return array;
        }

        //private float[] mean = new[] { 255f * 0.485f, 255f * 0.456f, 255f * 0.406f };

        //private float[] scale = new[] { 1 / (255f * 0.229f), 1 / (255f * 0.224f), 1 / (255f * 0.225f) };

    }
}

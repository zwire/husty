using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Husty;
using Husty.OnnxRuntime;
using Husty.OpenCvSharp;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace Test.YoloX
{

    public class YoloX : OnnxBase<Mat, YoloResult[]>
    {

        private readonly int[] _strides = new[] { 8, 16, 32 };
        private readonly Size _size;
        private readonly int _detectionCount;
        private readonly float _confThresh;
        private readonly string[] _labels;
        private readonly Mat _paddingBase;

        public YoloX(string model, string names, float confidenceThreshold = 0.5f) 
            : base(model, Provider.CPU, OptimizationLevel.Off) 
        {
            if (model.Contains("tiny") || model.Contains("nano"))
                _size = new(416, 416);
            else
                _size = new(640, 640);
            _confThresh = confidenceThreshold;
            _detectionCount = _strides.Select(x => _size.Width / x * _size.Height / x).Sum();
            _paddingBase = new(_size.Height, _size.Width, MatType.CV_8UC3, new(114, 114, 114));
            _labels = File.ReadAllLines(names);
        }


        public override YoloResult[] Run(Mat frame)
        {
            var wRatio = 1.0 / ((double)_size.Width / frame.Width);
            var hRatio = 1.0 / ((double)_size.Height / frame.Height);
            var ratio = Math.Max(wRatio, hRatio);
            if (wRatio < hRatio) wRatio = 1;
            else if (wRatio > hRatio) hRatio = 1;
            using var padded = _paddingBase.Clone();
            var size = new Size(frame.Width / ratio, frame.Height / ratio);
            padded[new Rect(0, 0, size.Width, size.Height)] = frame.Resize(size);
            var inputArray = CreateInputArray(padded);
            var outputArray = base.Run(inputArray)["output"].To2DJaggedArray(_detectionCount, _labels.Length + 5);

            var classIds = new List<int>();
            var confidences = new List<float>();
            var probabilities = new List<float>();
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
                        var p = outputArray[baseIndex + i * xGrids + j];
                        var index = p.Skip(5).ToArray().ArgMax(out var prob);
                        var conf = p[4];
                        if (conf * prob > _confThresh)
                        {
                            var x = (p[0] + j) / xGrids * hRatio;
                            var y = (p[1] + i) / yGrids * wRatio;
                            var w = Math.Exp(p[2]) / xGrids * hRatio;
                            var h = Math.Exp(p[3]) / yGrids * wRatio;
                            boxes.Add(new(x - w / 2, y - h / 2, w, h));
                            classIds.Add(index);
                            confidences.Add(conf);
                            probabilities.Add((float)prob);
                        }
                    }
                }
                baseIndex += xGrids * yGrids;
            }
            CvDnn.NMSBoxes(boxes, confidences, _confThresh, 0.3f, out int[] indices);
            return indices.Select(i => new YoloResult(boxes[i], confidences[i], _labels[classIds[i]], probabilities[i])).ToArray();
        }

        private unsafe float[] CreateInputArray(Mat frame)
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

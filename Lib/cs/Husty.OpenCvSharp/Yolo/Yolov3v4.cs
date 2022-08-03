using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace Husty.OpenCvSharp.Yolo
{
    /// <summary>
    /// Well-known object detection algorithm
    /// </summary>
    public sealed class Yolov3v4 : IYoloDetector
    {

        // ------ fields ------ //

        private readonly Net _net;
        private readonly Size _blobSize;
        private readonly float _confThresh;
        private readonly string[] _labels;
        private readonly SpinLock _locker;


        // ------ constructors ------ //

        /// <summary>
        /// Initialize detector (Darknet YOLO v3 or v4)
        /// </summary>
        /// <param name="cfg">(.cfg) file</param>
        /// <param name="weights">(.weights) file</param>
        /// <param name="names">(.names) file</param>
        /// <param name="blobSize">Must be multiple of 32</param>
        /// <param name="confidenceThreshold"></param>
        public Yolov3v4(string cfg, string weights, string names, Size blobSize, float confidenceThreshold = 0.5f)
        {
            if (blobSize.Width % 32 is not 0 || blobSize.Height % 32 is not 0)
                throw new ArgumentException("Blob width and height value must be multiple of 32.");
            if (confidenceThreshold < 0 || confidenceThreshold > 1)
                throw new ArgumentOutOfRangeException("must be 0.0 - 1.0");
            _locker = new();
            _confThresh = confidenceThreshold;
            _blobSize = blobSize;
            _net = CvDnn.ReadNetFromDarknet(cfg, weights);
            _labels = File.ReadAllLines(names);
        }


        // ------ public methods ------ //

        public YoloResult[] Run(Mat frame)
        {

            using var blob = CvDnn.BlobFromImage(frame, 1.0 / 255, _blobSize, default, true, false);
            Mat[] outs = null;
            _locker.Safeguard(() =>
            {
                if (!_net.IsDisposed)
                {
                    _net.SetInput(blob);
                    var outNames = _net.GetUnconnectedOutLayersNames();
                    outs = outNames.Select(_ => new Mat()).ToArray();
                    _net.Forward(outs, outNames);
                }
            });

            if (outs is null)
                return Array.Empty<YoloResult>();
            var ids = new List<int>();
            var confs = new List<float>();
            var probs = new List<float>();
            var boxes = new List<Rect2d>();
            unsafe
            {
                foreach (var pred in outs)
                {
                    var p = (float*)pred.Data;
                    for (var i = 0; i < pred.Rows; i++, p += pred.Cols)
                    {
                        if (p[4] > _confThresh)
                        {
                            Cv2.MinMaxLoc(pred.Row(i).ColRange(5, pred.Cols), out _, out var max, out _, out var maxLoc);
                            ids.Add(maxLoc.X);
                            confs.Add(p[4]);
                            probs.Add((float)max);
                            boxes.Add(new Rect2d(p[0] - p[2] / 2, p[1] - p[3] / 2, p[2], p[3]));
                        }
                    }
                    pred.Dispose();
                }
            }
            CvDnn.NMSBoxes(boxes, confs, _confThresh, 0.3f, out var indices);
            return indices.Select(i => new YoloResult(boxes[i], confs[i], _labels[ids[i]], probs[i])).ToArray();
        }

        public void Dispose()
        {
            _locker.Safeguard(_net.Dispose);
        }

    }
}
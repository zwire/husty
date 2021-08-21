using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace Husty.OpenCvSharp
{

    public enum DrawingMode { Off, Rectangle, Point }

    /// <summary>
    /// Well-known object detection algorithm
    /// </summary>
    public class Yolo
    {

        // ------- Fields ------- //

        private readonly DrawingMode _draw;
        private readonly Net _net;
        private readonly float _threshold;
        private readonly float _nmsThreshold;
        private readonly string[] _labels;
        private readonly Size _blobSize;
        private readonly Scalar[] _colors = Enumerable.Repeat(false, 80).Select(x => Scalar.RandomColor()).ToArray();


        // ------- Constructor ------- //

        /// <summary>
        /// Initialize detector
        /// </summary>
        /// <param name="cfg">(.cfg) file</param>
        /// <param name="names">(.names) file</param>
        /// <param name="weights">(.weights) file</param>
        /// <param name="blobSize">Must be multiple of 32</param>
        /// <param name="draw">Off or Point or Rectangle</param>
        /// <param name="confThresh">Threshold of 0.0-1.0 confidence value</param>
        /// <param name="nmsThresh">threshold of 0.0-1.0 NMS value</param>
        public Yolo(string cfg, string names, string weights, Size blobSize, DrawingMode draw = DrawingMode.Rectangle, float confThresh = 0.5f, float nmsThresh = 0.3f)
        {
            _blobSize = blobSize;
            _draw = draw;
            _threshold = confThresh;
            _nmsThreshold = nmsThresh;
            _labels = File.ReadAllLines(names).ToArray();
            _net = CvDnn.ReadNetFromDarknet(cfg, weights);
        }


        // ------- Methods ------- //

        /// <summary>
        /// Inference one frame
        /// </summary>
        /// <param name="frame">Input & output image</param>
        /// <param name="results">Holding 'YoloResult' class</param>
        public void Run(ref Mat frame, out YoloResults results)
        {
            var blob = CvDnn.BlobFromImage(frame, 1.0 / 255, _blobSize, new Scalar(), true, false);
            _net.SetInput(blob);
            var outNames = _net.GetUnconnectedOutLayersNames();
            var outs = outNames.Select(_ => new Mat()).ToArray();
            _net.Forward(outs, outNames);
            results = new YoloResults(GetResults(outs, frame));
        }

        private List<(string Label, float Confidence, Point Center, Size Size, Rect Box)> GetResults(Mat[] output, Mat image)
        {
            var classIds = new List<int>();
            var confidences = new List<float>();
            var probabilities = new List<float>();
            var centers = new List<Point>();
            var boxes = new List<Rect>();
            var w = image.Width;
            var h = image.Height;
            unsafe 
            {
                foreach (var pred in output)
                {
                    var p = (float*)pred.Data;
                    for (var i = 0; i < pred.Rows; i++, p += pred.Cols)
                    {
                        var confidence = p[4];
                        if (confidence > _threshold)
                        {
                            Cv2.MinMaxLoc(pred.Row(i).ColRange(5, pred.Cols), out _, out Point classIdPoint);
                            var probability = p[classIdPoint.X + 5];
                            if (probability > _threshold)
                            {
                                var centerX = (int)(p[0] * w);
                                var centerY = (int)(p[1] * h);
                                var width = (int)(p[2] * w);
                                var height = (int)(p[3] * h);
                                classIds.Add(classIdPoint.X);
                                confidences.Add(confidence);
                                probabilities.Add(probability);
                                centers.Add(new Point(centerX, centerY));
                                boxes.Add(new Rect(centerX - width / 2, centerY - height / 2, width, height));
                            }
                        }
                    }
                }
            }
            var results = new List<(string Label, float Confidence, Point Center, Size Size, Rect Rectangle)>();
            CvDnn.NMSBoxes(boxes, confidences, _threshold, _nmsThreshold, out int[] indices);
            foreach (var i in indices)
            {
                results.Add((_labels[classIds[i]], confidences[i], centers[i], new Size(boxes[i].Width, boxes[i].Height), boxes[i]));
                switch (_draw)
                {
                    case DrawingMode.Off:
                        break;
                    case DrawingMode.Point:
                        DrawPoint(image, classIds[i], centers[i]);
                        break;
                    case DrawingMode.Rectangle:
                        DrawRect(image, classIds[i], confidences[i], centers[i], new Size(boxes[i].Width, boxes[i].Height));
                        break;
                }
            }
            return results;
        }

        private void DrawPoint(Mat image, int classes, Point center)
        {
            image.Circle(center.X, center.Y, 3, _colors[classes], 5);
        }

        private void DrawRect(Mat image, int classes, float confidence, Point center, Size size)
        {
            var label = $"{_labels[classes]}{confidence * 100:0}%";
            var x1 = Math.Max(center.X - size.Width / 2, 0);
            var y1 = Math.Max(center.Y - size.Height / 2, 0);
            var x2 = Math.Min(center.X + size.Width / 2, image.Width);
            var y2 = Math.Min(center.Y + size.Height / 2, image.Height);
            Cv2.Rectangle(image, new Rect(x1, y1, x2 - x1, y2 - y1), _colors[classes], 2);
            var textSize = Cv2.GetTextSize(label, HersheyFonts.HersheyTriplex, 0.3, 0, out var baseline);
            Cv2.Rectangle(image, new Rect(new Point(x1, y1 - textSize.Height - baseline),
                new Size(textSize.Width, textSize.Height + baseline)), _colors[classes], Cv2.FILLED);
            var textColor = Cv2.Mean(_colors[classes]).Val0 < 70 ? Scalar.White : Scalar.Black;
            Cv2.PutText(image, label, new Point(x1, y1 - baseline), HersheyFonts.HersheyTriplex, 0.3, textColor);
        }

    }

    /// <summary>
    /// Accumulating detection results
    /// </summary>
    public class YoloResults : IEnumerable<(string Label, float Confidence, Point Center, Size Size, Rect Box)>, IEnumerator<(string Label, float Confidence, Point Center, Size Size, Rect Box)>
    {

        public YoloResults(IEnumerable<(string Label, float Confidence, Point Center, Size Size, Rect Box)> results)
        {
            Labels = new List<string>();
            Confidences = new List<float>();
            Centers = new List<Point>();
            Sizes = new List<Size>();
            Boxes = new List<Rect>();
            foreach (var result in results)
            {
                Labels.Add(result.Label);
                Confidences.Add(result.Confidence);
                Centers.Add(result.Center);
                Sizes.Add(result.Size);
                Boxes.Add(result.Box);
                Count++;
            }
        }


        private int position = -1;

        /// <summary>
        /// Class labels from (.names) file, such as 'person'.
        /// </summary>
        public List<string> Labels { private set; get; }

        /// <summary>
        /// Range of value is 0.0 ~ 1.0
        /// </summary>
        public List<float> Confidences { private set; get; }

        /// <summary>
        /// Detected center points
        /// </summary>
        public List<Point> Centers { private set; get; }

        /// <summary>
        /// Sizes of detected rectangles
        /// </summary>
        public List<Size> Sizes { private set; get; }

        /// <summary>
        /// Detected rectangles
        /// </summary>
        public List<Rect> Boxes { private set; get; }

        /// <summary>
        /// Detected count
        /// </summary>
        public int Count { private set; get; }

        public (string Label, float Confidence, Point Center, Size Size, Rect Rectangle) this[int i]
            => (Labels[i], Confidences[i], Centers[i], Sizes[i], Boxes[i]);

        public IEnumerator<(string Label, float Confidence, Point Center, Size Size, Rect Box)> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;

        public bool MoveNext() => ++position < Count;

        public (string Label, float Confidence, Point Center, Size Size, Rect Box) Current
            => (Labels[position], Confidences[position], Centers[position], Sizes[position], Boxes[position]);

        object IEnumerator.Current => Current;

        public void Dispose() { position = -1; }

        public void Reset() { position = -1; }

    }

}

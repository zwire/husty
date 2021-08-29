using System.IO;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace Husty.OpenCvSharp
{

    /// <summary>
    /// Well-known object detection algorithm
    /// </summary>
    public class YoloDetector
    {

        // ------- Fields ------- //

        private readonly Net _net;
        private readonly Size _blobSize;
        private readonly float _threshold;
        private readonly string[] _labels;


        // ------- Constructor ------- //

        /// <summary>
        /// Initialize detector
        /// </summary>
        /// <param name="cfg">(.cfg) file</param>
        /// <param name="names">(.names) file</param>
        /// <param name="weights">(.weights) file</param>
        /// <param name="blobSize">Must be multiple of 32</param>
        /// <param name="threshold"></param>
        public YoloDetector(string cfg, string names, string weights, Size blobSize, float threshold = 0.5f)
        {
            _threshold = threshold;
            _blobSize = blobSize;
            _net = CvDnn.ReadNetFromDarknet(cfg, weights);
            _labels = File.ReadAllLines(names);
        }


        // ------- Methods ------- //

        /// <summary>
        /// Inference one frame
        /// </summary>
        /// <param name="frame">input image</param>
        /// <returns></returns>
        public List<YoloResult> Run(Mat frame)
        {

            using var blob = CvDnn.BlobFromImage(frame, 1.0 / 255, _blobSize, new Scalar(), true, false);
            _net.SetInput(blob);
            var outNames = _net.GetUnconnectedOutLayersNames();
            var outs = outNames.Select(_ => new Mat()).ToArray();
            _net.Forward(outs, outNames);

            var classIds = new List<int>();
            var confidences = new List<float>();
            var boxes = new List<Rect>();
            var w = frame.Width;
            var h = frame.Height;
            unsafe 
            {
                foreach (var pred in outs)
                {
                    var p = (float*)pred.Data;
                    for (var i = 0; i < pred.Rows; i++, p += pred.Cols)
                    {
                        var confidence = p[4];
                        if (confidence > _threshold)
                        {
                            Cv2.MinMaxLoc(pred.Row(i).ColRange(5, pred.Cols), out _, out Point classIdPoint);
                            if (p[classIdPoint.X + 5] > _threshold)
                            {
                                var centerX = (int)(p[0] * w);
                                var centerY = (int)(p[1] * h);
                                var width = (int)(p[2] * w);
                                var height = (int)(p[3] * h);
                                classIds.Add(classIdPoint.X);
                                confidences.Add(confidence);
                                boxes.Add(new Rect(centerX - width / 2, centerY - height / 2, width, height));
                            }
                        }
                    }
                }
            }
            CvDnn.NMSBoxes(boxes, confidences, _threshold, 0.3f, out int[] indices);
            var results = new List<YoloResult>();
            foreach (var i in indices)
            {
                results.Add(new YoloResult
                {
                    Label = _labels[classIds[i]],
                    Confidence = confidences[i],
                    Box = boxes[i]
                });
            }
            return results;

        }

    }

}

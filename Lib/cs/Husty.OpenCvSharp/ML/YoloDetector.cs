using System;
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
        private readonly float _confidenceThreshold;
        private readonly string[] _labels;


        // ------- Constructor ------- //

        /// <summary>
        /// Initialize detector
        /// </summary>
        /// <param name="cfg">(.cfg) file</param>
        /// <param name="weights">(.weights) file</param>
        /// <param name="names">(.names) file</param>
        /// <param name="blobSize">Must be multiple of 32</param>
        /// <param name="confidenceThreshold"></param>
        public YoloDetector(string cfg, string weights, string names, Size blobSize, float confidenceThreshold = 0.5f)
        {
            if (blobSize.Width % 32 != 0 || blobSize.Height % 32 != 0)
                throw new ArgumentException("Blob width and height value must be multiple of 32.");
            if (confidenceThreshold < 0 || confidenceThreshold > 1)
                throw new ArgumentOutOfRangeException("must be 0.0 - 1.0");
            _confidenceThreshold = confidenceThreshold;
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
        public IReadOnlyList<YoloResult> Run(Mat frame)
        {

            using var blob = CvDnn.BlobFromImage(frame, 1.0 / 255, _blobSize, new Scalar(), true, false);
            _net.SetInput(blob);
            var outNames = _net.GetUnconnectedOutLayersNames();
            var outs = outNames.Select(_ => new Mat()).ToArray();
            _net.Forward(outs, outNames);

            var classIds = new List<int>();
            var confidences = new List<float>();
            var probabilityies = new List<float>();
            var boxes = new List<Rect2d>();
            unsafe 
            {
                foreach (var pred in outs)
                {
                    var p = (float*)pred.Data;
                    for (var i = 0; i < pred.Rows; i++, p += pred.Cols)
                    {
                        var confidence = p[4];
                        if (confidence > _confidenceThreshold)
                        {
                            Cv2.MinMaxLoc(pred.Row(i).ColRange(5, pred.Cols), out _, out Point classIdPoint);
                            var centerX = p[0];
                            var centerY = p[1];
                            var width = p[2];
                            var height = p[3];
                            classIds.Add(classIdPoint.X);
                            confidences.Add(confidence);
                            probabilityies.Add(p[classIdPoint.X + 5]);
                            boxes.Add(new Rect2d(centerX - width / 2, centerY - height / 2, width, height));
                        }
                    }
                    pred.Dispose();
                }
            }
            CvDnn.NMSBoxes(boxes, confidences, _confidenceThreshold, 0.3f, out int[] indices);
            return indices.Select(i => new YoloResult(_labels[classIds[i]], confidences[i], probabilityies[i], boxes[i])).ToArray();
        }

    }

}
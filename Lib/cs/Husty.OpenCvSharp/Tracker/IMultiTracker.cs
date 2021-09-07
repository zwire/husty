using System.Collections.Generic;
using OpenCvSharp;
using Husty.Filter;

namespace Husty.OpenCvSharp
{

    public enum OutputType { Correct, Predict }

    public interface IMultiTracker
    {

        /// <summary>
        /// Update status from observed input
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="detections">List of detected results</param>
        /// <param name="results"></param>
        public void Update(
            ref Mat frame,
            IEnumerable<(string Label, Point Center, Size Size)> detections,
            out IEnumerable<(int Id, string Label, float Iou, Point Center, Size Size, Rect Box)> results
            );

        /// <summary>
        /// Remove designated ID object.
        /// </summary>
        /// <param name="id">Target ID</param>
        public void Remove(int id);

    }


    /// <summary>
    /// For management by MultiTracker
    /// </summary>
    internal class Individual
    {

        // ------- Fields ------- //

        private readonly IFilter _filter;


        // ------- Properties ------- //

        internal string Label { set; get; }

        internal float Iou { set; get; }

        internal Point Center { set; get; }

        internal Size Size { set; get; }

        internal Point NextCenter { set; get; }

        internal Size NextSize { set; get; }

        internal int DetectCount { set; get; }

        internal int MissCount { set; get; }

        internal int Id { private set; get; }

        internal string Mark { private set; get; }


        // ------- Constructor ------- //

        internal Individual(Point center, Size size, int id, double dt, double filterStrength = 1.0, string label = "", string mark = "")
        {
            Id = id;
            Label = label;
            Center = center;
            Size = size;
            NextCenter = Center;
            NextSize = Size;
            Iou = 1f;
            DetectCount = 1;
            Mark = mark;
            var transitionMatrix = new double[]
                        {   1, 0, dt, 0, 0, 0,
                            0, 1, 0, dt, 0, 0,
                            0, 0, 1, 0, 0, 0,
                            0, 0, 0, 1, 0, 0,
                            0, 0, 0, 0, 1, 0,
                            0, 0, 0, 0, 0, 1  };
            var measurementMatrix = new double[]
                        {   1, 0, 0, 0, 0, 0,
                            0, 1, 0, 0, 0, 0,
                            0, 0, 0, 0, 1, 0,
                            0, 0, 0, 0, 0, 1  };
            var state = new double[] { Center.X, Center.Y, 0.0, 0.0, Size.Width, Size.Height };
            _filter = new Husty.Filter.KalmanFilter(state, transitionMatrix, measurementMatrix, filterStrength);
        }


        // ------- Methods ------- //

        internal void Predict(Point center, Size size)
        {
            var (correct, predict) = _filter.Update(new double[] { center.X, center.Y, size.Width, size.Height });
            Center = new Point(correct[0], correct[1]);
            Size = new Size(correct[4], correct[5]);
            NextCenter = new Point(predict[0], predict[1]);
            NextSize = new Size(predict[4], predict[5]);
        }

    }
}

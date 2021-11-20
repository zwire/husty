using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.ML;

namespace Husty.OpenCvSharp
{
    /// <summary>
    /// OpenCvSharp 'SVM' class wrapper.
    /// This class has accumulate & save & load methods for machine learning.
    /// </summary>
    public class Svm : Stats
    {

        // ------ fields ------ //

        private SVM _classifier;


        // ------ constructors ------ //

        /// <summary>
        /// OpenCvSharp 'SVM' class wrapper.
        /// This class has accumulate & save & load methods for machine learning.
        /// </summary>
        public Svm(Mode mode, string modelPath = "SvmModel.xml", string dataPath = "SvmTrainData.csv")
            : base(mode, modelPath, dataPath) { }


        // ------ public methods ------ //

        protected override void LoadModel() => _classifier = SVM.Load(_modelPath);

        public override void Train(bool append = true, double? param = 0.1)
        {
            if (_mode is not Mode.Train) throw new Exception("Mode should be 'Train'.");
            SaveDataset(append);
            using var svm = SVM.Create();
            svm.KernelType = SVM.KernelTypes.Rbf;
            svm.Gamma = (double)param;
            var list = new List<float>();
            _features.ForEach(f => list.AddRange(f));
            using var featureMat = new Mat(_features.Count, list.Count / _features.Count, MatType.CV_32F, list.ToArray());
            using var labelMat = new Mat(_labels.Count, 1, MatType.CV_32S, _labels.ToArray());
            svm.Train(featureMat, SampleTypes.RowSample, labelMat);
            svm.Save(_modelPath);
        }

        public override List<float> Predict(List<float[]> input)
        {
            if (_mode is not Mode.Inference) throw new  Exception("Mode should be 'Inference'.");
            var output = new List<float>();
            if (input.Count == 0)
                return output;
            using var inputMat = new Mat(input.Count, input[0].Length, MatType.CV_32F, input.SelectMany(i => i).ToArray());
            using var outputMat = new Mat();
            _classifier.Predict(inputMat, outputMat);
            for (int i = 0; i < outputMat.Rows; i++) output.Add(outputMat.At<float>(i, 0));
            return output;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.ML;

namespace Husty.OpenCvSharp
{
    /// <summary>
    /// OpenCvSharp 'BayesClassifier' class wrapper.
    /// This class has accumulate & save & load methods for machine learning.
    /// </summary>
    public class BayesClassifier : Stats
    {

        // ------ fields ------ //

        private NormalBayesClassifier _classifier;


        // ------ constructors ------ //

        /// <summary>
        /// OpenCvSharp 'BayesClassifier' class wrapper.
        /// This class has accumulate & save & load methods for machine learning.
        /// </summary>
        public BayesClassifier(Mode mode, string modelPath = "BayesModel.xml", string dataPath = "BayesTrainData.csv")
            : base(mode, modelPath, dataPath) { }


        // ------ public methods ------ //

        protected override void LoadModel() => _classifier = NormalBayesClassifier.Load(_modelPath);

        public override void Train(bool append = true, double? param = null)
        {
            if (_mode is not Mode.Train) throw new Exception("Mode should be 'Train'.");
            SaveDataset(append);
            using var bayes = NormalBayesClassifier.Create();
            var list = new List<float>();
            _features.ForEach(f => list.AddRange(f));
            using var featureMat = new Mat(_features.Count, list.Count / _features.Count, MatType.CV_32F, list.ToArray());
            using var labelMat = new Mat(_labels.Count, 1, MatType.CV_32S, _labels.ToArray());
            bayes.Train(featureMat, SampleTypes.RowSample, labelMat);
            bayes.Save(_modelPath);
        }

        public override List<float> Predict(List<float[]> input)
        {
            if (_mode is not Mode.Inference) throw new Exception("Mode should be 'Inference'.");
            var output = new List<float>();
            if (input.Count == 0)
                return output;
            using var inputMat = new Mat(input.Count, input[0].Length, MatType.CV_32F, input.SelectMany(i => i).ToArray());
            using var outputMat = new Mat();
            _classifier.Predict(inputMat, outputMat);
            for (int i = 0; i < outputMat.Rows; i++) output.Add(outputMat.At<float>(i, 0));
            return output;
        }

        public (List<float> Output, List<float> Probability) PredictProb(List<float[]> input)
        {
            if (_mode is not Mode.Inference) throw new Exception("Mode should be 'Inference'.");
            var output = new List<float>();
            var probability = new List<float>();
            if (input.Count == 0)
                return (output, probability);
            using var inputMat = new Mat(input.Count, input[0].Length, MatType.CV_32F, input.SelectMany(i => i).ToArray());
            using var outputMat = new Mat();
            using var probMat = new Mat();
            _classifier.PredictProb(inputMat, outputMat, probMat);
            for (int i = 0; i < outputMat.Rows; i++)
            {
                output.Add(outputMat.At<float>(i, 0));
                probability.Add(probMat.At<float>(i, 0));
            }
            return (output, probability);
        }

    }
}

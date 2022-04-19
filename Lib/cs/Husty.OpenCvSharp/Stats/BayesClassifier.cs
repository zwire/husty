using OpenCvSharp.ML;

namespace Husty.OpenCvSharp.Stats
{
    /// <summary>
    /// OpenCvSharp 'BayesClassifier' class wrapper.
    /// This class has accumulate & save & load methods for machine learning.
    /// </summary>
    public sealed class BayesClassifier : BinaryStatsBase
    {

        // ------ constructors ------ //

        /// <summary>
        /// OpenCvSharp 'BayesClassifier' class wrapper.
        /// This class has accumulate & save & load methods for machine learning.
        /// </summary>
        public BayesClassifier(string modelPath = null) : base(modelPath) { }


        // ------ inherited methods ------ //

        protected override StatModel DoLoadModel(string modelPath)
        {
            return NormalBayesClassifier.Load(modelPath);
        }

        protected override StatModel DoCreateDefaultModel()
        {
            return NormalBayesClassifier.Create();
        }

    }
}

using OpenCvSharp.ML;

namespace Husty.OpenCvSharp.Stats
{
    /// <summary>
    /// OpenCvSharp 'SVM' class wrapper.
    /// This class has accumulate & save & load methods for machine learning.
    /// </summary>
    public sealed class Svm : BinaryStatsBase
    {

        // ------ constructors ------ //

        /// <summary>
        /// OpenCvSharp 'SVM' class wrapper.
        /// This class has accumulate & save & load methods for machine learning.
        /// </summary>
        public Svm(string modelPath = null) : base(modelPath) { }


        // ------ inherited methods ------ //

        protected override StatModel DoLoadModel(string modelPath)
        {
            return SVM.Load(modelPath);
        }

        protected override StatModel DoCreateDefaultModel()
        {
            return SVM.Create();
        }

    }
}

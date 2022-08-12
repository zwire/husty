using OpenCvSharp;
using OpenCvSharp.ML;

namespace Husty.OpenCvSharp.Stats;

public abstract class BinaryStatsBase : IBinaryStats
{

    // ------ fields ------ //

    protected StatModel _classifier;


    // ------ constructors ------ //

    public BinaryStatsBase(string modelPath)
    {
        Load(modelPath);
    }


    // ------ public methods ------ //

    public void Load(string modelPath = "model.xml")
    {
        if (modelPath is not null)
            _classifier = DoLoadModel(modelPath);
        else
            _classifier = DoCreateDefaultModel();
    }

    public void Save(string modelPath)
    {
        _classifier.Save(modelPath);
    }

    public void Train(IEnumerable<StatsVectorData> dataset)
    {
        var ary = dataset.SelectMany(d => d.Feature).ToArray();
        using var featureMat = new Mat(dataset.Count(), ary.Length / dataset.Count(), MatType.CV_32F, ary);
        using var labelMat = new Mat(dataset.Count(), 1, MatType.CV_32S, dataset.Select(d => d.Label ? 1 : 0).ToArray());
        _classifier.Train(featureMat, SampleTypes.RowSample, labelMat);
    }

    public bool[] Predict(IEnumerable<IEnumerable<float>> input)
    {
        if (input.Count() is 0)
            throw new ArgumentException("Input data is empty!");
        using var inputMat = new Mat(input.Count(), input.FirstOrDefault().Count(), MatType.CV_32F, input.SelectMany(i => i).ToArray());
        using var outputMat = new Mat();
        _classifier.Predict(inputMat, outputMat);
        var output = new bool[outputMat.Rows];
        for (int i = 0; i < output.Length; i++)
            output[i] = outputMat.At<float>(i, 0) is 1;
        return output;
    }


    // ------ abstract methods ------ //

    protected abstract StatModel DoLoadModel(string modelPath);

    protected abstract StatModel DoCreateDefaultModel();

}

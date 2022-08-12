namespace Husty.NeuralNetwork;

internal static class OptimizerFactory
{
    public static IOptimizer Deserialize(string line)
    {
        var spt = line.Split(":<");
        var content = spt[1];
        return spt.FirstOrDefault() switch
        {
            "Sgd" => Sgd.Deserialize(content),
            "AdaGrad" => AdaGrad.Deserialize(content),
            "Adam" => Adam.Deserialize(content),
            _ => null
        };
    }
}

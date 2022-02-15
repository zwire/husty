namespace Husty.NeuralNetwork
{
    internal static class LayerFactory
    {

        public static ILayer Deserialize(string line)
        {
            var spt = line.Split("::");
            var content = spt[1..];
            return spt[0] switch
            {
                "Affine" => Affine.Deserialize(content),
                "Normalizer" => Normalizer.Deserialize(content),
                "Tanh" => Tanh.Deserialize(content),
                "Sigmoid" => Sigmoid.Deserialize(content),
                "Relu" => Relu.Deserialize(content),
                "LeakyRelu" => LeakyRelu.Deserialize(content),
                _ => null
            };
        }
    }
}

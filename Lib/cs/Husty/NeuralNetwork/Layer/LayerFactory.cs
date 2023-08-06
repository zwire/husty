namespace Husty.NeuralNetwork;

internal static class LayerFactory
{

  public static ILayer Deserialize(string line)
  {
    var spt = line.Split("::");
    var content = spt.Skip(1).ToArray();
    return spt.FirstOrDefault() switch
    {
      "Affine" => Affine.Deserialize(content),
      //"BatchNormalization" => BatchNormalization.Deserialize(content),
      "Tanh" => Tanh.Deserialize(content),
      "Sigmoid" => Sigmoid.Deserialize(content),
      "Relu" => Relu.Deserialize(content),
      "LeakyRelu" => LeakyRelu.Deserialize(content),
      _ => TryParseUnit(line)
    };
  }

  private static ILayer TryParseUnit(string line)
  {
    var spt = line.Split("<>");
    var content = spt.Skip(1).ToArray();
    if (spt.FirstOrDefault() is "Unit")
      return Unit.Deserialize(content);
    else
      return null;
  }

}

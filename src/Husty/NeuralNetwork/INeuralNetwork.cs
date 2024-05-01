namespace Husty.NeuralNetwork;

public interface INeuralNetwork
{

  public float[] Forward(float[] state);

  public void Backward(float[] error);

  public void Save(string name);

}

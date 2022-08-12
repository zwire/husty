using MathNet.Numerics.LinearAlgebra;

namespace Husty.NeuralNetwork;

public interface ILayer
{

    public Vector<float> Forward(Vector<float> x);

    public Vector<float> Backward(Vector<float> dout);

    public string Serialize();

}

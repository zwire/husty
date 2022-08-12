namespace DataArranger;

public interface IFunction
{

    public string GetFunctionExplanation();

    public string[] GetArgsExplanation();

    public void Run(string input, string output, string[] args);

}

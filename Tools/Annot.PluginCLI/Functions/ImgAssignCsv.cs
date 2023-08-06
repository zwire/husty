namespace Annot.PluginCLI;

public class ImgAssignCsv : IFunction
{

  public string GetFunctionExplanation()
  {
    return "assign images-masks and export csv file";
  }

  public string[] GetArgsExplanation()
  {
    return new[]
    {
            "input: original images folder path",
            "output: mask images folder path"
        };
  }

  public void Run(string input, string output, string[] args)
  {
    var x = Directory.GetFiles(input);
    var xDirs = input.Split("\\");
    using var sw = new StreamWriter($"{input}\\..\\DataSet.csv");
    sw.WriteLine("x:data,y:label");
    var y = Directory.GetFiles(output);
    var yDirs = output.Split("\\");
    for (int i = 0; i < y.Length; i++)
    {
      var xName = Path.GetFileName(x[i]);
      var yName = Path.GetFileName(y[i]);
      sw.WriteLine($"./{xDirs[^1]}/{xName},./{yDirs[^1]}/{yName}");
    }
  }
}

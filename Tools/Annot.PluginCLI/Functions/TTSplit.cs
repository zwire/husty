namespace Annot.PluginCLI;

public class TTSplit : IFunction
{

    public string GetFunctionExplanation()
    {
        return "split train-test (for csv format data)";
    }

    public string[] GetArgsExplanation()
    {
        return new[]
        {
            "input: input file path",
            "output: output folder path",
            "args[0]: test rate"
        };
    }

    public void Run(string input, string output, string[] args)
    {

        using var sw_train = new StreamWriter($"{output}\\Train.csv");
        using var sw_test = new StreamWriter($"{output}\\Test.csv");
        using var sr = new StreamReader(input);
        var strs = new List<string[]>();
        while (sr.Peek() is not -1)
            strs.Add(sr.ReadLine().Split(","));
        for (int i = 0; i < strs[0].Length; i++)
        {
            sw_train.Write($"{strs[0][i]}");
            sw_test.Write($"{strs[0][i]}");
            if (i != strs[0].Length - 1)
            {
                sw_train.Write(",");
                sw_test.Write(",");
            }
        }
        sw_train.WriteLine();
        sw_test.WriteLine();

        var count = 0;
        var testRate = double.Parse(args[0]);
        if (testRate < 0) testRate = 0.0;
        else if (testRate > 1) testRate = 1.0;
        for (int i = 1; i < strs.Count; i++)
        {
            if (count++ % (1.0 / testRate) is 0)
            {
                for (int j = 0; j < strs[i].Length; j++)
                {
                    sw_test.Write($"{strs[i][j]}");
                    if (j != strs[i].Length - 1)
                    {
                        sw_test.Write(",");
                    }
                }
                sw_test.WriteLine();
            }
            else
            {
                for (int j = 0; j < strs[i].Length; j++)
                {
                    sw_train.Write($"{strs[i][j]}");
                    if (j != strs[i].Length - 1)
                    {
                        sw_train.Write(",");
                    }
                }
                sw_train.WriteLine();
            }
        }
    }
}

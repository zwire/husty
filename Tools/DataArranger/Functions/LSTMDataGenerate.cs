namespace DataArranger;

// see https://piyonekochannel.com/entry/2020/02/11/211416
public class LSTMDataGenerate : IFunction
{

    public string GetFunctionExplanation()
    {
        return "export csv files for LSTM learning format";
    }

    public string[] GetArgsExplanation()
    {
        return new[]
        {
            "input: input csv file path",
            "output: output folder path",
            "args[0]: size of one segment"
        };
    }

    public void Run(string input, string output, string[] args)
    {
        var size = int.Parse(args[0]);
        var lines = File.ReadAllLines(input);
        if (!double.TryParse(lines[0].Split(",")[0], out _))
            lines = lines[1..];
        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);
        using var sw0 = new StreamWriter($"{output}\\dataset.csv");
        sw0.WriteLine("x:input,y:output");
        for (int i = 0; i < lines.Length - size - 1; i++)
        {
            sw0.WriteLine($"./z{i}.csv,{lines[i + size].Split(",")[^1]}");
            using var sw = new StreamWriter($"{output}\\z{i}.csv");
            foreach (var line in lines[i..(i + size)])
                sw.WriteLine(line);
        }
    }
}

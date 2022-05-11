using System.IO;

namespace Tools.NnDataArranger
{
    // see https://piyonekochannel.com/entry/2020/02/11/211416
    public static class LSTMDataGenerator
    {
        public static void Run(string inputFile, string outputDir, int size)
        {
            var lines = File.ReadAllLines(inputFile);
            if (!double.TryParse(lines[0].Split(",")[0], out _))
                lines = lines[1..];
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            using var sw0 = new StreamWriter($"{outputDir}\\dataset.csv");
            sw0.WriteLine("x:input,y:output");
            for (int i = 0; i < lines.Length - size - 1; i++)
            {
                sw0.WriteLine($"./z{i}.csv,{lines[i + size].Split(",")[^1]}");
                using var sw = new StreamWriter($"{outputDir}\\z{i}.csv");
                foreach (var line in lines[i..(i + size)])
                    sw.WriteLine(line);
            }
        }
    }
}

using OpenCvSharp;

namespace DataArranger;

public class HorizonalFlip : IFunction
{

    public string GetFunctionExplanation()
    {
        return "data argumentation";
    }

    public string[] GetArgsExplanation()
    {
        return new[]
        {
            "input: input images folder path",
            "output: output mask images folder path",
            "args[0]: rate"
        };
    }

    public void Run(string input, string output, string[] args)
    {
        var inputFiles = Directory.GetFiles(input);
        var maskFiles = Directory.GetFiles(output);
        if (inputFiles.Length != maskFiles.Length) return;
        var count = (int)(inputFiles.Length * double.Parse(args[0]));
        var skip = inputFiles.Length / count;
        var startIndex = new Random().Next(skip);
        for (int i = startIndex; i < inputFiles.Length; i += skip)
        {
            var inputFile = inputFiles[i];
            var maskFile = maskFiles[i];
            var img = Cv2.ImRead(inputFile);
            var mask = Cv2.ImRead(maskFile);
            Cv2.Flip(img, img, FlipMode.Y);
            Cv2.Flip(mask, mask, FlipMode.Y);
            Cv2.ImWrite(input + "\\flipped_" + Path.GetFileName(inputFile), img);
            Cv2.ImWrite(output + "\\flipped_" + Path.GetFileName(maskFile), mask);
        }
    }
}

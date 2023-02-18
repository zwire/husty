using OpenCvSharp;

namespace Annot.PluginCLI;

public class BrightnessDisperse : IFunction
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
            var hsv = img.CvtColor(ColorConversionCodes.BGR2HSV).Split();
            var rand = new Random().NextDouble() + 0.5;
            Cv2.ConvertScaleAbs(hsv[2], hsv[2], rand);
            Cv2.Merge(hsv, img);
            Cv2.CvtColor(img, img, ColorConversionCodes.HSV2BGR);
            Cv2.ImWrite(input + "\\bright_" + Path.GetFileName(inputFile), img);
            Cv2.ImWrite(output + "\\bright_" + Path.GetFileName(maskFile), mask);
        }
    }
}

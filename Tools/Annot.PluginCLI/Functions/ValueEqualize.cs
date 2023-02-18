using OpenCvSharp;

namespace Annot.PluginCLI;

public class ValueEqualize : IFunction
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
            Cv2.EqualizeHist(hsv[2], hsv[2]);
            Cv2.Merge(hsv, img);
            Cv2.CvtColor(img, img, ColorConversionCodes.HSV2BGR);
            Cv2.ImWrite(input + "\\equalized_" + Path.GetFileName(inputFile), img);
            Cv2.ImWrite(output + "\\equalized_" + Path.GetFileName(maskFile), mask);
        }
    }
}

using OpenCvSharp;

namespace Annot.PluginCLI;

public class ImgResize : IFunction
{

  public string GetFunctionExplanation()
  {
    return "resize images";
  }

  public string[] GetArgsExplanation()
  {
    return new[]
    {
            "input: input images folder path",
            "output: output images folder path",
            "args[0]: size (w, h)"
        };
  }

  public void Run(string input, string output, string[] args)
  {
    var files = Directory.GetFiles(input);
    var arg = args[0].Replace("(", "").Replace(")", "").Split(",");
    var size = new Size(int.Parse(arg[0]), int.Parse(arg[1]));
    foreach (var file in files)
    {
      var ex = Path.GetExtension(file);
      if (ex is not ".png" && ex is not ".jpg") continue;
      var names = file.Split("\\");
      var name = names[names.Length - 1];
      var img = Cv2.ImRead(file, ImreadModes.Unchanged);
      Cv2.Resize(img, img, size);
      Cv2.ImWrite($"{output}\\{name}", img);
    }
  }
}

using Husty.OpenCvSharp.CameraCalibration;
using Microsoft.Extensions.Hosting;

namespace CameraIntrinsicCalibrator;

internal class Program : ConsoleAppBase
{

  static async Task Main(string[] args)
  {
    await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);
  }

  public void Run(
      [Option("d", "working directory")] string dir = null,
      [Option("r", "rows of pettern")] int petternRows = 7,
      [Option("c", "cols of pettern")] int petternCols = 10
  )
  {
    dir ??= Environment.CurrentDirectory;
    if (!Directory.Exists(dir)) throw new ArgumentException("directory not exist!");
    var files = Directory.GetFiles(dir, "*.png");
    var param = IntrinsicCameraCalibrator.CalibrateWithChessboardImages(new(petternRows, petternCols, 32.5f), files);
    param.Save($"{dir}\\intrinsic.json");
    Console.WriteLine("successfully calibrated and saved as intrinsic.json");
  }
}


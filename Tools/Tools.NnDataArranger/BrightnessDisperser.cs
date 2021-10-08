using System;
using System.IO;
using OpenCvSharp;

namespace Tools.NnDataArranger
{
    internal static class BrightnessDisperser
    {
        internal static void Run(string inputFolder, string maskFolder, double rate)
        {
            var inputFiles = Directory.GetFiles(inputFolder);
            var maskFiles = Directory.GetFiles(maskFolder);
            if (inputFiles.Length != maskFiles.Length) return;
            var count = (int)(inputFiles.Length * rate);
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
                Cv2.ImWrite(inputFolder + "\\bright_" + Path.GetFileName(inputFile), img);
                Cv2.ImWrite(maskFolder + "\\bright_" + Path.GetFileName(maskFile), mask);
            }
        }
    }
}

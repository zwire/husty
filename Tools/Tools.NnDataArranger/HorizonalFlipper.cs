using System;
using System.IO;
using OpenCvSharp;

namespace Tools.NnDataArranger
{
    internal static class HorizonalFlipper
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
                Cv2.Flip(img, img, FlipMode.Y);
                Cv2.Flip(mask, mask, FlipMode.Y);
                Cv2.ImWrite(inputFolder + "\\flipped_" + Path.GetFileName(inputFile), img);
                Cv2.ImWrite(maskFolder + "\\flipped_" + Path.GetFileName(maskFile), mask);
            }
        }
    }
}

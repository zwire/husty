using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.NnDataArranger
{
    internal static class ValueEqualizer
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
                Cv2.EqualizeHist(hsv[2], hsv[2]);
                Cv2.Merge(hsv, img);
                Cv2.CvtColor(img, img, ColorConversionCodes.HSV2BGR);
                Cv2.ImWrite(inputFolder + "\\equalized_" + Path.GetFileName(inputFile), img);
                Cv2.ImWrite(maskFolder + "\\equalized_" + Path.GetFileName(maskFile), mask);
            }
        }
    }
}

﻿using System.IO;
using OpenCvSharp;

namespace Tools.NnDataArranger
{
    internal static class ImgResize
    {
        internal static void Run(string inputDir, string outputDir, Size outputSize)
        {
            var files = Directory.GetFiles(inputDir);
            foreach (var file in files)
            {
                var ex = Path.GetExtension(file);
                if (ex is not ".png" && ex is not ".jpg") continue;
                var names = file.Split("\\");
                var name = names[names.Length - 1];
                var img = Cv2.ImRead(file, ImreadModes.Unchanged);
                Cv2.Resize(img, img, outputSize);
                Cv2.ImWrite($"{outputDir}\\{name}", img);
            }
        }
    }
}

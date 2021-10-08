using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace Tools.NnDataArranger
{
    internal static class TTSplitFolder
    {
        internal static void Run(string dataDir, string maskDir, double testRate)
        {
            var xs = Directory.GetFiles(dataDir);
            var ys = Directory.GetFiles(maskDir);
            var dir = dataDir + "\\..\\";
            if (!Directory.Exists(dir + "train"))
                Directory.CreateDirectory(dir + "train");
            if (!Directory.Exists(dir + "trainannot"))
                Directory.CreateDirectory(dir + "trainannot");
            if (!Directory.Exists(dir + "val"))
                Directory.CreateDirectory(dir + "val");
            if (!Directory.Exists(dir + "valannot"))
                Directory.CreateDirectory(dir + "valannot");
            var count = 0;
            for (int i = 0; i < xs.Length; i++)
            {
                using var img = Cv2.ImRead(xs[i]);
                using var mask = Cv2.ImRead(ys[i], ImreadModes.Grayscale);
                if (count++ % (1.0 / testRate) is 0)
                {
                    Cv2.ImWrite(dir + "val\\" + Path.GetFileName(xs[i]), img);
                    Cv2.ImWrite(dir + "valannot\\" + Path.GetFileName(ys[i]), mask);
                }
                else
                {
                    Cv2.ImWrite(dir + "train\\" + Path.GetFileName(xs[i]), img);
                    Cv2.ImWrite(dir + "trainannot\\" + Path.GetFileName(ys[i]), mask);
                }
            }
        }
    }
}

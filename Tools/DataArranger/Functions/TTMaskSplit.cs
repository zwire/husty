using OpenCvSharp;

namespace DataArranger
{
    public class TTMaskSplit
    {

        public string GetFunctionExplanation()
        {
            return "assign images-masks and split train-test into folders (for mask COCO format)";
        }

        public string[] GetArgsExplanation()
        {
            return new[]
            {
                "input: original images folder path",
                "output: mask images folder path",
                "args[0]: test rate"
            };
        }

        public void Run(string input, string output, string[] args)
        {
            var xs = Directory.GetFiles(input);
            var ys = Directory.GetFiles(output);
            var dir = input + "\\..\\";
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
                if (count++ % (1.0 / int.Parse(args[0])) is 0)
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

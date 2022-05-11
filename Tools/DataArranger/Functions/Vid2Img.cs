using OpenCvSharp;

namespace DataArranger
{
    public class Vid2Img : IFunction
    {

        public string GetFunctionExplanation()
        {
            return "extract images from video sequence";
        }

        public string[] GetArgsExplanation()
        {
            return new[]
            {
                "input: input video file path",
                "output: output folder path",
                "args[0]: frame skip interval"
            };
        }

        public void Run(string input, string output, string[] args)
        {
            using var cap = new VideoCapture(input);
            cap.Set(VideoCaptureProperties.Fps, 1000);
            var count = 0;
            var imnum = 0;
            var img = new Mat();
            while (cap.Read(img))
            {
                if (count++ % int.Parse(args[0]) is 0)
                {
                    while (File.Exists($"{output}\\{imnum:d3}.png")) imnum++;
                    Cv2.ImWrite($"{output}\\{imnum:d3}.png", img);
                    Cv2.ImShow(" ", img);
                    Cv2.WaitKey(1);
                }
            }
        }
    }
}

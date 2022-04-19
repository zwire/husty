using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using Husty.OpenCvSharp.Stats;

namespace Test.Binarization
{
    class Program
    {

        private const string _model = "model.xml";
        private static int _width;
        private static int _height;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("\nTrain is 0. Inference is 1.");
                Console.Write("Input : ");
                var mode = int.Parse(Console.ReadLine());

                Console.WriteLine("Target image's path. The image must have 3 channels");
                Console.Write("Input : ");
                var path = Console.ReadLine();

                using var image = Cv2.ImRead(path);
                Cv2.Resize(image, image, new Size(640, 480));
                _width = image.Width;
                _height = image.Height;
                var bay = new Svm();

                if (mode is 0)
                {
                    Console.WriteLine("Select 'Positive' Areas. 'Enter' or 'Space' finishes current rectangle, 'Esc' determinates your selections.");
                    var positiveRects = Cv2.SelectROIs("Select 'Positive' Areas", image, false);
                    Console.WriteLine($"{positiveRects.Length} Rects is selected.");
                    Console.WriteLine();
                    Cv2.DestroyAllWindows();

                    Console.WriteLine("Select 'Negative' Areas. 'Enter' or 'Space' finishes current rectangle, 'Esc' determinates your selections.");
                    var negativeRects = Cv2.SelectROIs("Select 'Negative' Areas", image, false);
                    Console.WriteLine($"{negativeRects.Length} Rects is selected.");
                    Console.WriteLine();
                    Cv2.DestroyAllWindows();

                    var container = new DataContainer();
                    foreach (var r in positiveRects)
                        ImageToNormalizedList(image[r]).ForEach(bgr => container.AddData(bgr, true));
                    foreach (var r in negativeRects)
                        ImageToNormalizedList(image[r]).ForEach(bgr => container.AddData(bgr, false));
                    bay.Train(container.GetDataSet());
                    bay.Save(_model);
                }
                else if (mode is 1)
                {
                    bay.Load(_model);
                    var bgrs = ImageToNormalizedList(image);
                    var prob = bay.Predict(bgrs);
                    var img = ArrayToImage(prob);
                    Cv2.ImShow(" ", img);
                    Cv2.WaitKey();
                    Cv2.DestroyAllWindows();
                    img.Dispose();
                }
                Console.WriteLine();
                Console.WriteLine("'Finish' is 0. 'Continue' is 1.");
                Console.Write("Input : ");
                if (int.Parse(Console.ReadLine()) is not 1) break;
            }
        }

        unsafe private static List<float[]> ImageToNormalizedList(Mat image)
        {
            var d = image.DataPointer;
            var output = new List<float[]>();
            for (int i = 0; i < image.Width * image.Height; i++)
                output.Add(new float[] { d[i * 3 + 0] / 255f, d[i * 3 + 1] / 255f, d[i * 3 + 2] / 255f });
            return output;
        }

        unsafe private static Mat ArrayToImage(IEnumerable<bool> data)
        {
            var ary = data.ToArray();
            var gray = new Mat(_height, _width, MatType.CV_8U);
            var d = gray.DataPointer;
            for (int i = 0; i < _width * _height; i++)
                d[i] = (byte)(ary[i] ? 255 : 0);
            return gray;
        }
    }
}

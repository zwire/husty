using System;
using System.Threading.Tasks;
using Husty;
using OpenCvSharp;

namespace Test.MultiThreading
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var mole = new Channel<Mat>();
            var img = new Mat(1000, 1000, MatType.CV_8U, 0);
            Task.Run(async () =>
            {
                while (true)
                {
                    for (int i = 0; i < 10000; i++)
                        Cv2.Resize(img, img, new Size(1000, 1000));
                    var suc = await mole.WriteAsync(img);
                    if (!suc) break;
                    await Task.Delay(700);
                    Console.WriteLine("Write shitayo.");
                }
                Console.WriteLine("break write loop");
            });
            Task.Run(async () =>
            {
                while (true)
                {
                    var (suc, val) = await mole.ReadAsync();
                    if (!suc) break;
                    Console.WriteLine("Read shitayo.");
                    await Task.Delay(1);
                }
                Console.WriteLine("break read loop.");
            });
            Console.ReadKey();
            mole.Dispose();
            Console.ReadKey();
        }

    }
}

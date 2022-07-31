using Husty.Extensions;
using Husty.IO;

namespace MultiUdpSender
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var sender = new UdpSender(3000, 3001);
            var key = "A";
            var data = new double[] { 0, 1, 2 };
            Task.Run(async () =>
            {
                while (true)
                {
                    await sender.SendAsync(key, data);
                    Console.WriteLine("!");
                    Thread.Sleep(1000);
                }
            });
            ConsoleEx.WaitKey(ConsoleKey.Q, ConsoleKey.Escape);
        }
    }
}
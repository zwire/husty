using Husty.Extensions;
using Husty.IO;

namespace MultiUdpReceiver;

internal class Program
{
    static void Main(string[] args)
    {
        var receiver1 = new UdpReceiver(3000);
        var receiver2 = new UdpReceiver(3001);
        receiver1.GetStream<string>().Subscribe(x => Console.WriteLine(x));
        receiver2.GetStream<string>().Subscribe(x => Console.WriteLine(x));
        ConsoleEx.WaitKey(ConsoleKey.Q, ConsoleKey.Escape);
    }
}
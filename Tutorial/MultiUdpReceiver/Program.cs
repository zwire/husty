using Husty.Extensions;
using Husty.IO;

var receiver1 = new UdpReceiver(3000);
var receiver2 = new UdpReceiver(3001);
receiver1.GetStream<string>().Subscribe(x => Console.WriteLine(x));
receiver2.GetStream<string>().Subscribe(x => Console.WriteLine(x));
Console.WriteLine("Press Enter key to exit...");
ConsoleEx.WaitKey(ConsoleKey.Enter);
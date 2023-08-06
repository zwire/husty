using Husty.Communication;
using Husty.Extensions;

var receiver1 = new UdpDataTransporter().SetListeningPort(3000);
var receiver2 = new UdpDataTransporter().SetListeningPort(3001);
_ = Task.Run(async () =>
{
  while (true)
  {
    var (s1, d1) = await receiver1.TryReadLineAsync();
    var (s2, d2) = await receiver2.TryReadLineAsync();
    if (!s1 || !s2) break;
    Console.WriteLine(d1);
    Console.WriteLine(d2);
  }
});
Console.WriteLine("Press Enter key to exit...");
ConsoleEx.WaitKey(ConsoleKey.Enter);
using Husty.Communication;
using Husty.Extensions;

// var sock = new UdpDataTransporter().SetTargetPorts(new IPEndPoint(IPAddress.Broadcast, 3000), new IPEndPoint(IPAddress.Broadcast, 3001));
// overload methods can be input only port numbers and it has Loopback Address (127.0.0.1)
var sock = new UdpDataTransporter().SetTargetPorts(3000, 3001);
var data = "DATA";
_ = Task.Run(async () =>
{
  while (true)
  {
    if (!await sock.TryWriteLineAsync(data)) break;
    Console.WriteLine("!");
    Thread.Sleep(1000);
  }
});
Console.WriteLine("Press Enter key to exit...");
ConsoleEx.WaitKey(ConsoleKey.Enter);
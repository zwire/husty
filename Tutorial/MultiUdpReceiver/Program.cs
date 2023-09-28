using Husty.Communication;
using Husty.Extensions;

var receiver1 = new UdpDataTransporter().SetListeningPort(3000);
var receiver2 = new UdpDataTransporter().SetListeningPort(3001);
_ = Task.Run(async () =>
{
  while (true)
  {
    var result1 = await receiver1.TryReadLineAsync();
    var result2 = await receiver2.TryReadLineAsync();
    if (!result1.IsOk || !result2.IsOk) break;
    Console.WriteLine(result1.Unwrap());
    Console.WriteLine(result2.Unwrap());
  }
});
Console.WriteLine("Press Enter key to exit...");
ConsoleEx.WaitKey(ConsoleKey.Enter);
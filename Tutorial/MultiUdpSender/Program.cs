using Husty.Extensions;
using Husty.IO;

var sock = new UdpDataTransporter(2999, 3000, 3001);
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
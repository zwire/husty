using Husty.IO;

Console.WriteLine("waiting for connection ...");
var server = await WebSocketDataTransporter.CreateServerAsync("127.0.0.1", 8000);
Console.WriteLine("connected!");
var counter = 0;
while (!(Console.KeyAvailable && Console.ReadKey().Key is ConsoleKey.Escape) && server.IsOpened)
{
    var msg = $"Hello {counter++}";
    await server.TryWriteLineAsync(msg);
    Console.WriteLine("-->" + msg);
    var (success, data) = await server.TryReadLineAsync();
    if (success)
        Console.WriteLine("<--" + data);
    else
        break;
    Thread.Sleep(1000);
}
server.Dispose();
Console.WriteLine("completed");
Console.ReadKey();
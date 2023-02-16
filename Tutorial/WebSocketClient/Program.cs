using Husty.IO;

Console.WriteLine("attempt to connect ...");
var client = await WebSocketDataTransporter.CreateClientAsync("127.0.0.1", 8000);
Console.WriteLine("connected!");
while (!(Console.KeyAvailable && Console.ReadKey().Key is ConsoleKey.Escape) && client.IsOpened)
{
    var (success, data) = await client.TryReadLineAsync();
    if (success)
        Console.WriteLine("<--" + data);
    await client.TryWriteLineAsync("Hi!");
    Console.WriteLine("-->");
}
client.Dispose();
Console.WriteLine("completed");
Console.ReadKey();
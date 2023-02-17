using Husty.Extensions;
using Husty.Communication;

Console.WriteLine("attempt to connect ...");
var client = await WebSocketDataTransporter.CreateClientAsync("127.0.0.1", 8000);
Console.WriteLine("connected!");

var cts = new CancellationTokenSource();
ObservableEx2
    .Loop(i =>
    {
        var t = client.ReadLine();
        if (t is null) cts.Cancel();
        Console.WriteLine("<-- " + t);
        var msg = $"Hello {i}";
        client.TryWriteLine(msg);
        Console.WriteLine("--> " + msg);
    }, default, cts.Token);
ConsoleEx.WaitKey(ConsoleKey.Enter);
cts.Cancel();

client.Dispose();
Console.WriteLine("completed");
Console.ReadKey();
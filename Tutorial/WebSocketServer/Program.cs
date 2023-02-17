using Husty.Extensions;
using Husty.Communication;

Console.WriteLine("waiting for connection ...");
var server = await WebSocketDataTransporter.CreateServerAsync("127.0.0.1", 8000);
Console.WriteLine("connected!");

var cts = new CancellationTokenSource();
ObservableEx2
    .Loop(() =>
    {
        var msg = "Hi!";
        server.TryWriteLine(msg);
        Console.WriteLine("--> " + msg);
        var t = server.ReadLine();
        if (t is null) cts.Cancel();
        Console.WriteLine("<-- " + t);
    }, default, cts.Token);
ConsoleEx.WaitKey(ConsoleKey.Enter);
cts.Cancel();

server.Dispose();
Console.WriteLine("completed");
Console.ReadKey();
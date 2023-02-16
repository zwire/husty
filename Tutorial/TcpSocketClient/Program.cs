using Husty.Extensions;

// initialize
var client = new Husty.IO.TcpSocketClient("127.0.0.1", 5000);
var (success, stream) = client.GetStream();
if (!success) return;

// loop
var count = 0;
while (true)
{
    var snd = new Message("Hello", count++);
    if (!await stream.TryWriteAsJsonAsync(snd)) break;
    Console.WriteLine(snd);
    if (ConsoleEx.WaitKey() is ConsoleKey.Enter) break;
}

// finalize
stream.Dispose();
client.Dispose();

// define and share send/receive object
internal record Message(string Greeting, int Number);

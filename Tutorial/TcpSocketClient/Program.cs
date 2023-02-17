using Husty.Communication;
using Husty.Extensions;

// initialize
var client = new TcpSocketClient("127.0.0.1", 5000);
var (success, stream) = client.GetStream();
if (!success) return;

// loop
var cts = new CancellationTokenSource();
ObservableEx2
    .Loop(i => stream.TryWriteAsJson(new Message("Hello", i)), TimeSpan.FromSeconds(1), default, cts.Token);
ConsoleEx.WaitKey(ConsoleKey.Enter);

// finalize
cts.Cancel();
stream.Dispose();
client.Dispose();

// define and share send/receive object
internal record Message(string Greeting, int Number);

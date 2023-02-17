using System.Reactive.Linq;
using Husty.Extensions;
using Husty.Communication;

// initialize
var server = new TcpSocketServer(5000);
var (success, stream) = await server.GetStreamAsync();
if (!success) return;

// loop
var cts = new CancellationTokenSource();
ObservableEx2
    .Loop(stream.ReadAsJson<Message>, default, cts.Token)
    .Where(x => x is not null)
    .Subscribe(x => Console.WriteLine($"{x.Greeting} : {x.Number}"));
ConsoleEx.WaitKey(ConsoleKey.Enter);

// finalize
cts.Cancel();
stream.Dispose();
server.Dispose();

// define and share send/receive object
internal record Message(string Greeting, int Number);

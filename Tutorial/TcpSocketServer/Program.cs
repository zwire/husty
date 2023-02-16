// initialize

var server = new Husty.IO.TcpSocketServer(5000);
var (success, stream) = server.GetStream();
if (!success) return;

// loop
while (true)
{
    var (has, rcv) = await stream.TryReadAsJsonAsync<Message>();
    if (!has) break;
    Console.WriteLine($"{rcv.Greeting} : {rcv.Number}");
    if (Console.KeyAvailable && Console.ReadKey().Key is ConsoleKey.Enter)
        break;
}

// finalize
stream.Dispose();
server.Dispose();

// define and share send/receive object
internal record Message(string Greeting, int Number);

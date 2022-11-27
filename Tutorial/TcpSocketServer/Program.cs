namespace TcpSocketServer;

// define and share send/receive object
internal record Message(string Greeting, int Number);

internal class Program
{
    static void Main(string[] args)
    {

        // initialize
        var server = new Husty.IO.TcpSocketServer(5000);
        var stream = server.GetStream();

        // loop
        while(true)
        {
            var rcv = stream.ReadAsJson<Message>();
            if (rcv is null) break;
            Console.WriteLine($"{rcv.Greeting} : {rcv.Number}");
            if (Console.KeyAvailable && Console.ReadKey().Key is ConsoleKey.Enter)
                break;
        }

        // finalize
        stream.Dispose();
        server.Dispose();

    }
}


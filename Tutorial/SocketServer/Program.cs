using System;
using Husty.IO;

namespace SocketServer
{

    // define and share send/receive object
    internal record Message(string Greeting, int Number);

    internal class Program
    {
        static void Main(string[] args)
        {

            // initialize
            var server = new TcpSocketServer(5000);
            var stream = server.GetStream();

            // loop
            while(true)
            {
                var rcv = stream.ReadAsJson<Message>();
                if (rcv is null) break;
                Console.WriteLine($"{rcv.Greeting} : {rcv.Number}");
                if (Console.KeyAvailable)
                    if (Console.ReadKey().Key is ConsoleKey.Q)
                        break;
            }

            // finalize
            stream.Dispose();
            server.Dispose();

        }
    }

}

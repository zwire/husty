using System;
using Husty.IO;

namespace SocketClient
{

    // define and share send/receive object
    internal record Message(string Greeting, int Number);

    internal class Program
    {
        static void Main(string[] args)
        {

            // initialize
            var client = new TcpSocketClient("127.0.0.1", 5000);
            var stream = client.GetStream();

            // loop
            var count = 0;
            while (true)
            {
                var snd = new Message("Hello", count++);
                stream.WriteAsJson(snd);
                Console.WriteLine(snd);
                if (Console.ReadKey().Key is ConsoleKey.Q)
                    break;
            }

            // finalize
            stream.Dispose();
            client.Dispose();

        }
    }
}

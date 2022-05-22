using System.Text;
using Husty.IO;

namespace WebSocketServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("waiting for connection ...");
            var server = new WebSocketStream(WebSocketType.Server, "127.0.0.1", 8000);
            Console.WriteLine("connected!");
            var counter = 0;
            while (!(Console.KeyAvailable && Console.ReadKey().Key is ConsoleKey.Escape) && server.IsOpened)
            {
                var msg = $"Hello {counter++}";
                server.Write(msg, Encoding.UTF8);
                Console.WriteLine("-->" + msg);
                var res = server.Read();
                Console.WriteLine("<--" + res.FirstOrDefault());
                Thread.Sleep(1000);
            }
            server.Close();
            Console.WriteLine("completed");
            Console.ReadKey();
        }
    }
}
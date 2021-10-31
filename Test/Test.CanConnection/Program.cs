using System;
using System.Threading;
using Husty.Lawicel;

namespace Test.CanConnection
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var names = CanUsbAdapter.FindAdapterNames();
            var sender = new CanUsbWriter(names[0]);
            sender.Open();
            Console.WriteLine("Open");
            while (true)
            {
                sender.Write(new(0, 0, 0, 0, 0));
                Console.WriteLine("Write");
                Thread.Sleep(1000);
                if (Console.KeyAvailable)
                    if (Console.ReadKey().Key is ConsoleKey.Q)
                        break;
            }
            sender.Close();
            Console.WriteLine("Close");
            Console.ReadKey();
        }
    }
}

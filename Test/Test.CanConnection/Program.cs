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
            var receiver = new CanUsbReader(names[0]);
            var sender = new CanUsbWriter(names[0]);
            receiver.Open();
            sender.Open();
            Console.WriteLine("Open");

            receiver.ReadAtInterval(TimeSpan.FromSeconds(1))
                .Subscribe(msg => Console.WriteLine(msg is null ? "null" : msg.Id));


            while (true)
            {
                sender.Write(new(0, 0, 0, 0, 0));
                Thread.Sleep(1000);
                if (Console.KeyAvailable)
                    if (Console.ReadKey().Key is ConsoleKey.Q)
                        break;
            }

            receiver.Close();
            sender.Close();
            Console.WriteLine("Close");
            Console.ReadKey();
        }
    }
}

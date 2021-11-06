using System;
using System.Reactive.Linq;
using Husty.Lawicel;

namespace Test.CanConnection
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var names = CanUsbAdapter.FindAdapterNames();
            var adapter = new CanUsbAdapter(names[0], CanUsbOption.BAUD_250K);
            adapter.Open();
            Console.WriteLine("Open");

            adapter.GetReadingStream()
                .Sample(TimeSpan.FromMilliseconds(10))
                .Subscribe(msg =>
                {
                    Console.WriteLine(msg is null ? "null" : msg.Id);
                });

            Console.ReadKey();

            //while (true)
            //{
            //    adapter.Write(new(0, 0, 0, 0, 0));
            //    Thread.Sleep(1000);
            //    if (Console.KeyAvailable)
            //        if (Console.ReadKey().Key is ConsoleKey.Q)
            //            break;
            //}

            adapter.Dispose();
            Console.WriteLine("Close");
            Console.ReadKey();
        }
    }
}

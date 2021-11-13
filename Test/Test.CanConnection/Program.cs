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
            var adapter = new CanUsbAdapter(names[0], CanUsbOption.BAUD_250K);
            adapter.Open();
            Console.WriteLine("Open");

            while (true)
            {
                adapter.Write(new(0x0CFD43F7, 999999999));
                Thread.Sleep(1000);
                if (Console.KeyAvailable)
                    if (Console.ReadKey().Key is ConsoleKey.Q)
                        break;
            }

            adapter.Dispose();
            Console.WriteLine("Close");
            Console.ReadKey();
        }
    }
}

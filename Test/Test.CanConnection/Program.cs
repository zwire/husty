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
            receiver.Open();
            Console.WriteLine("Open");

            receiver.ReadAtInterval(TimeSpan.FromSeconds(1))
                .Subscribe(msg => Console.WriteLine(msg is null ? "null" : msg.Id));

            Console.ReadKey();

            //while (true)
            //{
            //    var rcv = receiver.Read();
            //    if (rcv is null) continue;
            //    Console.WriteLine("Read ID : " + rcv.Id);
            //    Thread.Sleep(1000);
            //    if (Console.KeyAvailable)
            //        if (Console.ReadKey().Key is ConsoleKey.Q)
            //            break;
            //}

            receiver.Close();
            Console.WriteLine("Close");
            Console.ReadKey();
        }
    }
}

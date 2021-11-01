using System;
using Husty.IO;

namespace Tutorial.SerialPortReader
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // get ports
            var names = SerialPort.GetPortNames();
            if (names.Length is 0) return;

            // access first found port
            var port = new SerialPort(names[0], 115250);
            var stream = port.GetStream();

            // read messages until key interrupt
            while(true)
            {
                Console.WriteLine(stream.ReadString());
                if (Console.KeyAvailable)
                    if (Console.ReadKey().Key is ConsoleKey.Q)
                        break;
            }
            Console.WriteLine("completed.");
            stream.Dispose();

        }
    }
}

using System;
using Husty.IO;

namespace PythonStdoutReader
{
    internal class Program
    {

        const string pythonExe = "";
        const string pythonFile = "";

        static void Main(string[] args)
        {
            var reader = new StdOutReader(pythonExe, pythonFile);
            reader.GetStream().Subscribe(x => Console.WriteLine(x));
            while (Console.ReadKey().Key is not ConsoleKey.Escape) ;
            reader.Dispose();
        }
    }
}

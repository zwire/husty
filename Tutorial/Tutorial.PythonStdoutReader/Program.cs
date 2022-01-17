using System;
using Husty;

namespace Tutorial.PythonStdoutReader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var pythonExe = "";
            var pythonFile = "";
            var reader = new StdOutReader(pythonExe, pythonFile);
            reader.GetStream().Subscribe(x => Console.WriteLine(x));
            while (Console.ReadKey().Key is not ConsoleKey.Escape) ;
        }
    }
}

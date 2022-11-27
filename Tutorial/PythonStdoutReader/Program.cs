using Husty.Extensions;
using Husty.IO;

namespace PythonStdoutReader;

internal class Program
{

    const string pythonExe = "";
    const string pythonFile = "";

    static void Main(string[] args)
    {
        var reader = new StdOutReader(pythonExe, pythonFile);
        reader.GetStream().Subscribe(x => Console.WriteLine(x));
        Console.WriteLine("Press Enter key to exit...");
        ConsoleEx.WaitKey(ConsoleKey.Enter);
        reader.Dispose();
    }
}

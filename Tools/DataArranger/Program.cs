using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace DataArranger;

internal class Program : ConsoleAppBase
{

    static async Task Main(string[] args)
    {
        await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);
        Console.WriteLine("\nTo see all functions, input '-f functions' option.");
    }

    public void Run(
        [Option("f", "function type name")] string function,
        [Option("i", "input file / folder path")] string input = null,
        [Option("o", "output file / folder path")] string output = null,
        [Option("a", "args (split by space)")] string arguments = null
    )
    {
        if (function is "functions")
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var t in types)
            {
                if (t.GetInterfaces().Any(x => x == typeof(IFunction)))
                {
                    var f = Activator.CreateInstance(t) as IFunction;
                    Console.WriteLine($"◆ {t.Name}: {f.GetFunctionExplanation()}");
                    Console.WriteLine("    args: ");
                    foreach (var ex in f.GetArgsExplanation())
                        Console.WriteLine("\t" + ex);
                    Console.WriteLine();
                }
            }
            return;
        }
        try
        {
            (Activator.CreateInstance(Type.GetType(function)) as IFunction).Run(input, output, arguments.Split(' '));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}

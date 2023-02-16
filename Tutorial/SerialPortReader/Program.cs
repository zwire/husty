using Husty.IO;

// get ports
var names = SerialPortDataTransporter.GetPortNames();
if (names.Length is 0) throw new Exception("find no port!");

// access first found port
var port = new SerialPortDataTransporter(names[0], 115250);

// read messages until key interrupt
while (true)
{
    var (success, data) = await port.TryReadLineAsync();
    if (success) Console.WriteLine(data);
    if (Console.KeyAvailable && Console.ReadKey().Key is ConsoleKey.Enter)
        break;
}

// finalize
Console.WriteLine("completed.");
port.Dispose();
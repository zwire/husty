using Husty.Lawicel;

var can = new CanUsbAdapter(null, CanUsbOption.BAUD_500K);
can.Open();

while (true)
{
    Console.WriteLine(can.Read().Data.ToString("X"));
    Thread.Sleep(100);
    if (Console.KeyAvailable && Console.ReadKey().Key is ConsoleKey.Enter) break;
}

can.Close();
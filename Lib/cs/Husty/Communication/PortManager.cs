using System.Diagnostics;

namespace Husty.Communication;

public struct Baudrate
{
  public const int BAUD_110 = 110;
  public const int BAUD_300 = 300;
  public const int BAUD_600 = 600;
  public const int BAUD_1200 = 1200;
  public const int BAUD_2400 = 2400;
  public const int BAUD_4800 = 4800;
  public const int BAUD_9600 = 9600;
  public const int BAUD_14400 = 14400;
  public const int BAUD_19200 = 19200;
  public const int BAUD_38400 = 38400;
  public const int BAUD_57600 = 57600;
  public const int BAUD_115200 = 115200;
  public const int BAUD_230400 = 230400;
  public const int BAUD_460800 = 460800;
  public const int BAUD_921600 = 921600;
}

public static class PortManager
{

  // ------ public methods ------ //

  public static async Task<string> SearchPortAsync(int baudrate, params string[] keyPetterns)
  {
    foreach (var p in SerialPortDataTransporter.GetPortNames())
    {
      try
      {
        using var port = new SerialPortDataTransporter(p, baudrate, System.IO.Ports.StopBits.One, default, default, 500);
        foreach (var key in keyPetterns)
        {
          for (int i = 0; i < 10; i++)
          {
            try
            {
              var r = await port.TryReadLineAsync().ConfigureAwait(false);
              if (!r.IsOk) break;
              if (r.Unwrap().Contains(key) is true)
              {
                Debug.WriteLine($"find port: '{p}' for {keyPetterns.Aggregate((line, k) => line += $"{k} ")}");
                return p;
              }
            }
            catch
            {
              break;
            }
          }
        }
      }
      catch { }
    }
    throw new HustyInternalException($"failed to open port for {keyPetterns.Aggregate((line, k) => line += $"{k} ")}");
  }

}

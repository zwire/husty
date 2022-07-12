using System;
using System.Diagnostics;
using System.Linq;

namespace Husty.IO
{
    public struct Baudrate
    {
        public const int BAUD_110        = 110;
        public const int BAUD_300        = 300;
        public const int BAUD_600        = 600;
        public const int BAUD_1200       = 1200;
        public const int BAUD_2400       = 2400;
        public const int BAUD_4800       = 4800;
        public const int BAUD_9600       = 9600;
        public const int BAUD_14400      = 14400;
        public const int BAUD_19200      = 19200;
        public const int BAUD_38400      = 38400;
        public const int BAUD_57600      = 57600;
        public const int BAUD_115200     = 115200;
        public const int BAUD_230400     = 230400;
        public const int BAUD_460800     = 460800;
        public const int BAUD_921600     = 921600;
    }

    public static class PortManager
    {

        // ------ public methods ------ //

        public static string SearchPort(int baudrate, params string[] keyPetterns)
        {
            foreach (var p in SerialPort.GetPortNames())
            {
                try
                {
                    using var port = new SerialPort(p, baudrate, 500);
                    foreach (var key in keyPetterns)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            try
                            {
                                var line = port.ReadLine();
                                if (line is null) break;
                                if (line.Contains(key) is true)
                                {
                                    Debug.WriteLine($"find port: '{p}' for {keyPetterns.Aggregate((line, k) => line += $"{k} ")}");
                                    return p;
                                }
                            }
                            catch (TimeoutException e)
                            {
                                break;
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
            return null;
        }

    }
}

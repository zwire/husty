using System;
using System.IO.Ports;

namespace Husty.IO
{
    public class SerialPort : ICommunicator
    {

        // ------- Fields ------- //

        private readonly System.IO.Ports.SerialPort _port;


        // ------- Constructor ------- //

        public SerialPort(string portName, int baudRate)
        {
            try
            {
                _port = new();
                _port.PortName = portName;
                _port.BaudRate = baudRate;
                _port.StopBits = StopBits.One;
                _port.Handshake = Handshake.None;
                _port.Parity = Parity.None;
                _port.Open();
            }
            catch
            {
                throw new Exception("failed to open!");
            }
        }


        // ------- Methods ------- //

        public BidirectionalDataStream GetStream()
        {
            var stream = _port.BaseStream;
            return new(stream, stream);
        }
        
        public void Dispose()
        {
            _port?.Close();
            _port?.Dispose();
        }

        public static string[] GetPortNames()
        {
            try
            {
                return System.IO.Ports.SerialPort.GetPortNames();
            }
            catch
            {
                return new string[] { "null" };
            }
        }

    }
}

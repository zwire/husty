using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Husty
{
    public class SerialPort : ICommunicator
    {

        // ------- fields ------- //

        private readonly System.IO.Ports.SerialPort _port;


        // ------- constructors ------- //

        public SerialPort(string portName, int baudRate, int readTimeout = -1, int writeTimeout = -1)
        {
            _port = new();
            _port.PortName = portName;
            _port.BaudRate = baudRate;
            _port.StopBits = StopBits.One;
            _port.Handshake = Handshake.None;
            _port.Parity = Parity.None;
            _port.ReadTimeout = readTimeout;
            _port.WriteTimeout = writeTimeout;
            try
            {
                _port.Open();
            }
            catch
            {
                throw new Exception("failed to open!");
            }
        }


        // ------- public methods ------- //

        public BidirectionalDataStream GetStream()
        {
            return new(_port.BaseStream, _port.BaseStream);
        }

        public async Task<BidirectionalDataStream> GetStreamAsync()
        {
            return await Task.Run(() => GetStream());
        }

        public void Dispose()
        {
            _port?.Close();
            _port?.Dispose();
        }

        public static string[] GetPortNames()
        {
            return System.IO.Ports.SerialPort.GetPortNames();
        }
    }
}

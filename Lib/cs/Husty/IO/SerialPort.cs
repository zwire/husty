using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Husty.IO
{
    public sealed class SerialPort : ICommunicator
    {

        // ------- fields ------- //

        private bool _disposed;
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
            return new(_port.BaseStream, _port.BaseStream, _port.WriteTimeout, _port.ReadTimeout);
        }

        public async Task<BidirectionalDataStream> GetStreamAsync()
        {
            return await Task.FromResult(GetStream()).ConfigureAwait(false);
        }

        public bool Write(string value)
        {
            if (_disposed) return false;
            try
            {
                if (_port?.IsOpen is true) _port?.Write(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool WriteLine(string value)
        {
            if (_disposed) return false;
            try
            {
                if (_port?.IsOpen is true) _port?.WriteLine(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string ReadLine()
        {
            if (_disposed) return null;
            try
            {
                return _port?.IsOpen is true ? _port?.ReadLine() : null;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _port?.Close();
            _port?.Dispose();
        }

        public static string[] GetPortNames()
        {
            return System.IO.Ports.SerialPort.GetPortNames();
        }

    }
}

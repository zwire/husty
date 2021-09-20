using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Husty.IO
{
    public class NamedPipeServer : ICommunicator
    {

        // ------- Fields ------- //

        private readonly NamedPipeServerStream _writer;
        private readonly NamedPipeServerStream _reader;


        // ------- Constructors ------- //

        public NamedPipeServer(string pipeName)
        {
            _reader = new(pipeName + "ClientToServer", PipeDirection.In);
            _writer = new(pipeName + "ServerToClient", PipeDirection.Out);
            Task.Run(() =>
            {
                try
                {
                    _reader.WaitForConnection();
                    _writer.WaitForConnection();
                }
                catch
                {
                    throw new Exception("failed to connect!");
                }
            });
        }


        // ------- Methods ------- //

        public bool WaitForConnect()
        {
            return WaitForConnectAsync().Result;
        }

        public async Task<bool> WaitForConnectAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    while (!_writer.IsConnected) ;
                    while (!_reader.IsConnected) ;
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public BidirectionalDataStream GetStream()
        {
            return GetStreamAsync().Result;
        }

        public async Task<BidirectionalDataStream> GetStreamAsync()
        {
            return await Task.Run(() =>
            {
                WaitForConnect();
                return new BidirectionalDataStream(_writer, _reader);
            });
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _reader?.Dispose();
        }

    }
}

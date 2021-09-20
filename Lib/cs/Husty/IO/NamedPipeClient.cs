using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Husty.IO
{
    public class NamedPipeClient : ICommunicator
    {

        // ------- Fields ------- //

        private readonly NamedPipeClientStream _writer;
        private readonly NamedPipeClientStream _reader;


        // ------- Constructors ------- //

        public NamedPipeClient(string pipeName, string serverName = ".")
        {
            _writer = new(serverName, pipeName + "ClientToServer", PipeDirection.Out);
            _reader = new(serverName, pipeName + "ServerToClient", PipeDirection.In);
            Task.Run(() =>
            {
                try
                {
                    _writer.Connect();
                    _reader.Connect();
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

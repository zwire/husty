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
        private readonly Task _connectionTask;


        // ------- Constructors ------- //

        public NamedPipeClient(string pipeName, string serverName = ".")
        {
            _writer = new(serverName, pipeName + "ClientToServer", PipeDirection.Out);
            _reader = new(serverName, pipeName + "ServerToClient", PipeDirection.In);
            _connectionTask = Task.Run(() =>
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

        public BidirectionalDataStream GetStream()
        {
            return GetStreamAsync().Result;
        }

        public async Task<BidirectionalDataStream> GetStreamAsync()
        {
            return await Task.Run(() =>
            {
                _connectionTask.Wait();
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

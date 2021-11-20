using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Husty
{
    public class NamedPipeClient : ICommunicator
    {

        // ------ fields ------ //

        private readonly NamedPipeClientStream _writer;
        private readonly NamedPipeClientStream _reader;
        private readonly Task _connectionTask;


        // ------ properties ------ //

        public int ReadTimeout { set; get; } = -1;

        public int WriteTimeout { set; get; } = -1;


        // ------ constructors ------ //

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


        // ------ public methods ------ //

        public BidirectionalDataStream GetStream()
        {
            _connectionTask.Wait();
            return new BidirectionalDataStream(_writer, _reader, WriteTimeout, ReadTimeout);
        }

        public async Task<BidirectionalDataStream> GetStreamAsync()
        {
            return await Task.Run(() => GetStream());
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _reader?.Dispose();
        }

    }
}

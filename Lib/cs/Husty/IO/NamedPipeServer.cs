using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Husty
{
    public class NamedPipeServer : ICommunicator
    {

        // ------ fields ------ //

        private readonly NamedPipeServerStream _writer;
        private readonly NamedPipeServerStream _reader;
        private readonly Task _connectionTask;


        // ------ properties ------ //

        public int ReadTimeout { set; get; } = -1;

        public int WriteTimeout { set; get; } = -1;


        // ------ constructors ------ //

        public NamedPipeServer(string pipeName)
        {
            _reader = new(pipeName + "ClientToServer", PipeDirection.In);
            _writer = new(pipeName + "ServerToClient", PipeDirection.Out);
            _connectionTask = Task.Run(() =>
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

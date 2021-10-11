using System;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Threading;

namespace Husty
{
    public class Channel<T> : IDisposable
    {

        // ------ Fields ------ //

        private readonly System.Threading.Channels.Channel<T> _channel;
        private readonly CancellationTokenSource _wcts;
        private readonly CancellationTokenSource _rcts;


        // ------ Properties ------ //

        public int ReadTimeout { private set; get; }

        public int WriteTimeout { private set; get; }


        // ------ Constructors ------ //

        public Channel(int readTimeout = -1, int writeTimeout = -1)
        {
            ReadTimeout = readTimeout;
            WriteTimeout = writeTimeout;
            _rcts = new CancellationTokenSource(ReadTimeout);
            _wcts = new CancellationTokenSource(WriteTimeout);
            _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = true,
            });
        }


        // ------ Methods ------ //

        public async Task<bool> WriteAsync(T item)
        {
            if (_wcts.IsCancellationRequested)
            {
                _channel.Writer.Complete();
                return false;
            }
            try
            {
                await _channel.Writer.WaitToWriteAsync();
                await _channel.Writer.WriteAsync(item);
                return true;
            }
            catch
            {
                _channel.Writer.Complete();
                return false;
            }
        }

        public async Task<(bool Success, T Value)> ReadAsync()
        {
            if (_rcts.IsCancellationRequested)
            {
                return (false, default);
            }
            try
            {
                await _channel.Reader.WaitToReadAsync(_rcts.Token);
                var value = await _channel.Reader.ReadAsync(_rcts.Token);
                return (true, value);
            }
            catch
            {
                return (false, default);
            }
        }

        public void Dispose()
        {
            _rcts.Cancel();
            _wcts.Cancel();
        }

    }
}

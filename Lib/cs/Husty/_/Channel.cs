using System;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Threading;

namespace Husty
{
    public sealed class Channel<T> : IDisposable
    {

        // ------ fields ------ //

        private readonly System.Threading.Channels.Channel<T> _channel;
        private readonly CancellationTokenSource _wcts;
        private readonly CancellationTokenSource _rcts;


        // ------ properties ------ //

        public int ReadTimeout { set; get; } = -1;

        public int WriteTimeout { set; get; } = -1;


        // ------ constructors ------ //

        public Channel(int capacity = 1)
        {
            _rcts = new CancellationTokenSource(ReadTimeout);
            _wcts = new CancellationTokenSource(WriteTimeout);
            _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = true,
            });
        }


        // ------ public methods ------ //

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

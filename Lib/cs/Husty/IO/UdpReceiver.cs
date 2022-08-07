using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;

namespace Husty.IO
{
    public class UdpReceiver
    {

        // ------ fields ------ //

        private readonly UdpClient _sock;
        private readonly IObservable<KeyValuePair<string, string>> _observable;
        private IPEndPoint _ep = null;
        private bool _closed;


        // ------ properties ------ //

        public Encoding Encoding { get; } = Encoding.UTF8;


        // ------ constructors ------ //

        public UdpReceiver(int port)
        {
            _sock = new UdpClient(port);
            var observable = Observable.Repeat(0, ThreadPoolScheduler.Instance)
                .TakeUntil(_ => _closed)
                .Finally(_sock.Close)
                .Select(_ =>
                {
                    var str = Encoding.GetString(_sock.Receive(ref _ep));
                    if (str.Length < 3) return default;
                    var len = int.Parse(str[..2]);
                    var key = str.Substring(2, len);
                    var value = str[(2 + len)..];
                    return new KeyValuePair<string, string>(key, value);
                })
                .Publish();
            observable.Connect();
            _observable = observable;
        }


        // ----- public methods ------ //

        public IObservable<T?> GetStream<T>(string? key = null)
        {
            return _observable
                .Where(x => key is null || x.Key == key)
                .Select(x => JsonSerializer.Deserialize<T>(x.Value));
        }

        public void Close()
        {
            _closed = true;
        }

    }
}

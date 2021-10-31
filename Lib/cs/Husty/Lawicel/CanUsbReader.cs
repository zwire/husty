using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Reactive.Bindings;
using static Husty.Lawicel.CANUSB;
using static Husty.Lawicel.CanMessage;

namespace Husty.Lawicel
{
    public class CanUsbReader : CanUsbAdapter
    {

        private IDisposable _connector;

        public ReactivePropertySlim<CanMessage> ReactiveMessage { private set; get; }


        public CanUsbReader(string adapterName) : base(adapterName)
        {
            ReactiveMessage = new();
        }

        public void Open()
        {
            OpenAdapter();
            _connector = Observable.Repeat(0, ThreadPoolScheduler.Instance)
                .Finally(() => CloseAdapter())
                .Subscribe(_ =>
                {
                    var ret = canusb_Read(Handle, out var message);
                    if (ret is not ERROR_OK) throw new Exception("Failed to receive the message.");
                    ReactiveMessage.Value = FromCANMsg(message);
                });
        }

        public CanMessage Read()
        {
            return ReactiveMessage.Value;
        }

        public IObservable<CanMessage> ReadAtInterval(TimeSpan interval)
        {
            return Observable.Interval(interval)
                .Select(_ => ReactiveMessage.Value).Publish().RefCount();
        }

        public override void Close()
        {
            _connector?.Dispose();
            CloseAdapter();
        }

    }
}

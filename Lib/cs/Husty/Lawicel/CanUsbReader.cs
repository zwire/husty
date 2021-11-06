using System;
using System.Reactive.Linq;
using static Husty.Lawicel.CANUSB;
using static Husty.Lawicel.CanMessage;

namespace Husty.Lawicel
{
    public class CanUsbReader : CanUsbAdapter
    {

        public CanUsbReader(string adapterName, string baudrate = BAUD_500K) : base(adapterName, baudrate)
        {
            OpenAdapter();
        }

        public IObservable<CanMessage> ReadAtInterval(TimeSpan interval)
        {
            return Observable.Interval(interval)
                .Finally(() => CloseAdapter())
                .Select(_ =>
                {
                    var ret = canusb_Read(Handle, out var message);
                    if (ret is not ERROR_OK) throw new Exception("Failed to receive the message.");
                    return FromCANMsg(message);
                })
                .Publish()
                .RefCount();
        }

        public override void Close()
        {
            CloseAdapter();
        }

    }
}

using System;
using static Husty.Lawicel.CANUSB;
using static Husty.Lawicel.CanMessage;

namespace Husty.Lawicel
{
    public class CanUsbWriter : CanUsbAdapter
    {

        public CanUsbWriter(string adapterName, string baudrate = BAUD_500K) : base(adapterName, baudrate)
        {

        }

        public void Open()
        {
            OpenAdapter();
        }

        public override void Close()
        {
            CloseAdapter();
        }

        public void Write(CanMessage message)
        {
            ThrowIfDisposed();
            ThrowIfNotOnline();
            var msg = message.ToCANMsg();
            var ret = canusb_Write(Handle, ref msg);
            if (ret is not ERROR_OK) throw new Exception("Failed to send the message.");
        }

    }
}

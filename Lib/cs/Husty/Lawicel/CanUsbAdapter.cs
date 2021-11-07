using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using static Husty.Lawicel.CANUSB;
using static Husty.Lawicel.CanUsbOption;

namespace Husty.Lawicel
{

    public enum CanUsbStatus { Offline, Online }


    public class CanUsbAdapter : IDisposable
    {

        private bool _disposed;
        private object _locker = new();
        private IDisposable _readingConnector;
        private IDisposable _writingConnector;

        public uint Handle { get; private set; }

        public string AdapterName { get; }

        public string Baudrate { get; }

        public CanUsbStatus Status { private set; get; } = CanUsbStatus.Offline;


        public CanUsbAdapter(string adapterName, string baudrate)
        {
            AdapterName = adapterName;
            Baudrate = baudrate;
        }

        public static string[] FindAdapterNames()
        {

            var adapterList = new List<string>();
            var buf = new StringBuilder(32);
            int numAdapters = canusb_getFirstAdapter(buf, 32);
            if (numAdapters is 0) return adapterList.ToArray();
            adapterList.Add(buf.ToString());
            for (int i = 1; i < numAdapters; i++)
                if (canusb_getNextAdapter(buf, 32) > 0)
                    adapterList.Add(buf.ToString());
            return adapterList.ToArray();

        }

        public void Open()
        {
            ThrowIfDisposed();
            ThrowIfNotOffline();
            Handle = canusb_Open(
                AdapterName,
                Baudrate,
                ACCEPTANCE_CODE_ALL, ACCEPTANCE_MASK_ALL,
                FLAG_TIMESTAMP | FLAG_BLOCK
            );
            if (Handle <= 0) throw new Exception("Failed to open the CANUSB Adapter.");
            Status = CanUsbStatus.Online;
        }

        public void Close()
        {
            if (_disposed) return;
            if (Status is CanUsbStatus.Offline) return;
            _readingConnector?.Dispose();
            _writingConnector?.Dispose();
            var ret = canusb_Close(Handle);
            if (ret is not ERROR_OK) throw new Exception("Failed to close the CANUSB adapter");
            Status = CanUsbStatus.Offline;
        }


        public void Write(CanMessage message)
        {
            ThrowIfDisposed();
            ThrowIfNotOnline();
            var msg = message.ToCANMsg();
            lock(_locker)
            {
                canusb_Flush(Handle, Convert.ToByte(FLUSH_WAIT));
                var ret = canusb_Write(Handle, ref msg);
                if (ret is not ERROR_OK) throw new Exception("Failed to send the message.");
            }
        }

        public CanMessage Read()
        {
            lock(_locker)
            {
                var ret = canusb_Read(Handle, out var message);
                if (ret is not ERROR_OK) throw new Exception("Failed to receive the message.");
                return CanMessage.FromCANMsg(message);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Close();
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        protected void ThrowIfNotOffline()
        {
            if (Status is not CanUsbStatus.Offline)
                throw new InvalidOperationException("Must be invoked on Offline status.");
        }

        protected void ThrowIfNotOnline()
        {
            if (Status is not CanUsbStatus.Online)
                throw new InvalidOperationException("Must be invoked on Online status.");
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

    }

}

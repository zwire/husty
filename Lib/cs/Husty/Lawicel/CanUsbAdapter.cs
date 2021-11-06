using System;
using System.Text;
using System.Collections.Generic;
using static Husty.Lawicel.CANUSB;
using static Husty.Lawicel.CanMessage;

namespace Husty.Lawicel
{

    public enum CanUsbStatus { Offline, Online }


    public abstract class CanUsbAdapter : IDisposable
    {

        private bool _disposed;

        public uint Handle { get; private set; }

        public string AdapterName { get; }

        public string Baudrate { get; }

        public CanUsbStatus Status { private set; get; } = CanUsbStatus.Offline;


        protected CanUsbAdapter(string adapterName, string baudrate)
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

        public abstract void Close();

        protected void OpenAdapter()
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

        protected void CloseAdapter()
        {
            if (_disposed) return;
            if (Status is CanUsbStatus.Offline) return;
            var ret = canusb_Close(Handle);
            if (ret is not ERROR_OK) throw new Exception("Failed to close the CANUSB adapter");
            Status = CanUsbStatus.Offline;
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


        public void Dispose()
        {
            if (!_disposed)
            {
                CloseAdapter();
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

    }

}

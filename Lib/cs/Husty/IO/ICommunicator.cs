using System;
using System.Threading.Tasks;

namespace Husty.IO
{
    public interface ICommunicator : IDisposable
    {

        public bool WaitForConnect();

        public Task<bool> WaitForConnectAsync();

        public BidirectionalDataStream GetStream();

        public Task<BidirectionalDataStream> GetStreamAsync();

    }
}

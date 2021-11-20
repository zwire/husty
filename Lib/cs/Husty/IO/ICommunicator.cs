using System;
using System.Threading.Tasks;

namespace Husty
{
    public interface ICommunicator : IDisposable
    {

        public BidirectionalDataStream GetStream();

        public Task<BidirectionalDataStream> GetStreamAsync();

    }
}

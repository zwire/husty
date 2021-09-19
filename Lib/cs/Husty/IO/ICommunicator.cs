using System;

namespace Husty.IO
{
    public interface ICommunicator : IDisposable
    {

        public BidirectionalDataStream GetStream();

    }
}

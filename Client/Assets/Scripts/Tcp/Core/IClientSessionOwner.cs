using System;

namespace Tcp.Core
{
    public interface IClientSessionOwner
    {
        void Disconnected(Guid key);
    }
}
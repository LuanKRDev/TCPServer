using System;
using Tcp.Core.Message;

namespace Tcp.Core
{
    public interface ISessionHandler : IDisposable
    {
        Guid Key { get; }
        void Send(IFormattableMessage message);
        bool IsConnected();
    }
}
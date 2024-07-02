using System;
using Tcp.Core.Message;

namespace Tcp.Core
{
    public interface ISessionHandler : IDisposable
    {
        Guid Id { get; }
        void Send(IFormattableMessage message);
    }
}
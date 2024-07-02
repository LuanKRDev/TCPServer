using System;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace Tcp.Core.Abstracts
{
    public abstract class AbstractClientSessionHandler : AbstractSessionHandler, IClientSessionHandler
    {
        [CanBeNull] private IClientSessionOwner _owner;
        [CanBeNull] public IClientSessionOwner Owner => _owner;

        public virtual void Start(IClientSessionOwner owner, Guid id, Socket socket)
        {
            this._owner = owner;
            base.Start(id, socket);
        }
    }
}
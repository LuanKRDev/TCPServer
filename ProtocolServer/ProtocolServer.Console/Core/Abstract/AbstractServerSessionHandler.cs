using ProtocolServer.Console.Core;
using System.Net.Sockets;

namespace ProtocolServer.Console.Core.Abstract
{
    public abstract class AbstractServerSessionHandler : AbstractSessionHandler, IServerSessionHandler
    {
        private IServerSessionOwner? owner;
        public IServerSessionOwner? Owner => owner;

        public virtual void Start(IServerSessionOwner owner, Guid key, Socket socket)
        {
            this.owner = owner;
            base.Start(key, socket);
        }


        protected override void OnDisconnect()
        {
            Owner?.DisconnectTo(this.Key);
        }
    }
}


namespace ProtocolServer.Console.Core
{
    public interface IServerSessionOwner
    {
        void DisconnectTo(Guid key);
        void SendMessageTo(Guid key, dynamic message);
        void SendMessageToAll(dynamic message);
        void SendMessageToAllExcept(Guid key, dynamic message);
    }
}

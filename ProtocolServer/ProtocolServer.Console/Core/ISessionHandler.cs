namespace ProtocolServer.Console.Core
{
    public interface ISessionHandler : IDisposable
    {
        Guid Key { get; }
        void Send(IFormattableMessage message);
    }
}

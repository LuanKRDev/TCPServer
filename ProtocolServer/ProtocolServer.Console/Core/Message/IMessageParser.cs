namespace ProtocolServer.Console.Core.Message
{
    public interface IMessageParser
    {
        IMessage Parse(ReadOnlySpan<byte> buffer);
    }
}

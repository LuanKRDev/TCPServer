using ProtocolServer.Logging.Enums;

namespace ProtocolServer.Logging
{
    public static class Logger
    {
        public static void Log(LogType type, string message)
        {
            Console.WriteLine($"{GetType(type)}: {message}");
        }

        private static string GetType(LogType type) 
            => type switch { 
                LogType.Log => "[Log]", 
                LogType.Warn => "[Warn]", 
                LogType.Error => "[Error]", 
                _ => "Unknown" };
    }
}

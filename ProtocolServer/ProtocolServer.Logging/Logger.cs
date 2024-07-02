using System;
using ProtocolServer.Logging.Enums;

namespace ProtocolServer.Logging
{
    public static class Logger
    {
        public static void Log(LogType type, string message)
        {
            string logTypeString = GetType(type);
            string coloredMessage = $"{logTypeString}: {message}";
            ConsoleColor originalColor = Console.ForegroundColor;
            
            switch (type)
            {
                case LogType.Log:
                    Console.ForegroundColor = ConsoleColor.DarkCyan; // Cor branca para logs
                    break;
                case LogType.Warn:
                    Console.ForegroundColor = ConsoleColor.Yellow; // Cor amarela para avisos
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red; // Cor vermelha para erros
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Gray; // Cor cinza para tipos desconhecidos
                    break;
            }

            Console.WriteLine(coloredMessage);
            Console.ForegroundColor = originalColor;
        }

        private static string GetType(LogType type)
        {
            switch (type)
            {
                case LogType.Log:
                    return "[Log]";
                case LogType.Warn:
                    return "[Warn]";
                case LogType.Error:
                    return "[Error]";
                default:
                    return "Unknown";
            }
        }
    }
}
using System;
using System.Diagnostics;

namespace HowardPlays
{
    static class Debug
    {
        public static void Log(object message, bool includeTimestamp = true)
        {
            if (includeTimestamp)
                Console.WriteLine($"{Utility.Timestamp} {message}\n");
            else
                Console.WriteLine(message + "\n");
        }

        public static void LogWarning(object message)
        {
            Console.WriteLine($"{Utility.Timestamp} WARN: {message}\n");
        }

        public static void LogError(object message)
        {
            Console.WriteLine($"{Utility.Timestamp} ERROR: {message}\n{new StackTrace()}\n");
        }
    }
}
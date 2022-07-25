using System;
using System.Diagnostics;

namespace HowardBot
{
    static class Debug
    {
        /// <summary>
        /// Logs a message to the console.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="includeTimestamp">If the timestamp should be included.</param>
        public static void Log(object message, bool includeTimestamp = true)
        {
            if (includeTimestamp)
                Console.WriteLine($"{Utility.Timestamp} {message}\n");
            else
                Console.WriteLine(message + "\n");
        }

        /// <summary>
        /// Logs a warning to the console.
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void LogWarning(object message)
        {
            Console.WriteLine($"{Utility.Timestamp} WARN: {message}\n");
        }

        /// <summary>
        /// Logs an error to the console.
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void LogError(object message)
        {
            Console.WriteLine($"{Utility.Timestamp} ERROR: {message}\n{new StackTrace()}\n");
        }
    }
}
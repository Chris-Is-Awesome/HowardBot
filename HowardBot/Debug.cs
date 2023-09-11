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
		public static void Log(object message, bool includeTimestamp = true, ConsoleColor color = ConsoleColor.White, bool addNewLine = true)
		{
			string output = includeTimestamp ? $"{Utility.Timestamp} {message}" : message.ToString();

			// Add new line
			if (addNewLine)
				output += "\n";

			Console.ForegroundColor = color;
			Console.WriteLine(output);
			Console.ResetColor();
		}

		/// <summary>
		/// Logs a warning to the console.
		/// </summary>
		/// <param name="message">The message to log</param>
		public static void LogWarning(object message)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine($"{Utility.Timestamp} {message}\n");
			Console.ResetColor();
		}

		/// <summary>
		/// Logs an error to the console.
		/// </summary>
		/// <param name="message">The message to log</param>
		public static void LogError(object message, bool postStackTrace = true)
		{
			string output = $"{Utility.Timestamp} {message}\n";

			if (postStackTrace)
				output += new StackTrace() + "\n";

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(output);
			Console.ResetColor();
		}
	}
}
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// Shoutout to this post: https://stackoverflow.com/a/22996552/20961168 for allowing cleanup before exit

namespace HowardBot
{
	class Program
	{
		static bool exitSystem = false;

		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

		private delegate bool EventHandler(CtrlType sig);
		static EventHandler _closeHandler;

		enum CtrlType
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}

		private static void Main()
		{
			AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

			Console.Title = "HowardBot";

			Debug.Log($"[Bot started at {DateTime.Now}]", false);

			// Initialize bot
			Bot bot = Bot.Instance;

			_closeHandler += new EventHandler(HandleCloseEvent);
			SetConsoleCtrlHandler(_closeHandler, true);

			// Keep persistent
			while (!exitSystem)
			{
				Thread.Sleep(500);
			}
		}

		private static bool HandleCloseEvent(CtrlType sig)
		{
			Debug.Log("Closing due to CTRL+C or process termination");

			Cleanup();

			return true;
		}

		private static async void Cleanup()
		{
			Debug.Log("Cleaning up...");
			await Bot.OnBotClose();
			Debug.Log("Cleanup complete, shutting down now");

			// Allow main to run off
			exitSystem = true;

			// Shutdown right away so there are no lingering threads
			Environment.Exit(-1);
		}

		// Create crash log in the event of a crash
		private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			TwitchHandler.SendMessage("/me *crashes*");
			Exception ex = (Exception)e.ExceptionObject;
			string timestamp = DateTime.Now.ToString("MM-dd-yy_HH-mm-ss");
			string fileName = $"HowardBot_Crash_{timestamp}.txt";
			string stack = ex.StackTrace.Replace(Utility.CurrentDirectory + @"\", "");
			StringBuilder output = new();
			output.AppendLine($"Unhandled exception occurred in {ex.Source} at {DateTime.Now}!");
			output.AppendLine($"Message: {ex.Message}");
			output.AppendLine($"Stack:\n{stack}");
			Utility.WriteToFile($@"{Utility.CurrentDirectory}\Crash Logs", fileName, output.ToString());
		}
	}
}
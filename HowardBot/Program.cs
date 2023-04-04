using System;
using System.Runtime.InteropServices;
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
        static EventHandler _handler;

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
            Console.Title = "HowardBot";

            Debug.Log($"[Bot started at {DateTime.Now}]");

            // Initialize bot
            Bot bot = Bot.Instance;

            // Stuff (idk)
			_handler += new EventHandler(Handler);
			SetConsoleCtrlHandler(_handler, true);

			// Keep persistent
			while (!exitSystem)
            {
                Thread.Sleep(500);
            }
        }

        private static bool Handler(CtrlType sig)
        {
			Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

            //do your cleanup here
            Cleanup();
			Thread.Sleep(2500); //simulate some cleanup delay

			Console.WriteLine("Cleanup complete, shutting down now");
            Bot.SendMessage("/me Gracefully enters the barn and goes to sleep.");

			//allow main to run off
			exitSystem = true;

			//shutdown right away so there are no lingering threads
			Environment.Exit(-1);

			return true;
		}

        private static async void Cleanup()
        {
            Debug.Log("Cleaning up...");
            await RewardHandler.Instance.DeleteCustomRewards();
		}
    }
}

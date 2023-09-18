using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AutoHotkey.Interop;

namespace HowardBot
{
	class Bot : Singleton<Bot>
	{
		private readonly Stopwatch initTimer;
		private bool connectedToOBS;
		private bool hasStarted; // Used to know if the bot has done initialization already in case of a reconnect
		private string streamLogsDir;
		private string streamLogFileName;
		private string streamLogFullPath;
		private DateTime streamStartTime;

		/// <summary>
		/// Set to true to test live events without needing to be live
		/// </summary>
		public static bool TestLiveStuff { get; } = true; // Set to true to test reward redemptions
		public static bool AmILive { get; private set; }
		public static bool IsInChannel { get; private set; }
		public static AutoHotkeyEngine AHK { get; private set; }
		public static API API { get; private set; }
		public static AudioPlayer AudioPlayer { get; private set; }
		public static MessageHandler MessageHandler { get; private set; }
		public static OBSHandler OBSHandler { get; set; }
		public static TwitchHandler TwitchHandler { get; private set; }
		public static RewardHandler RewardHandler { get; private set; }

		public static string HowardToken { get; private set; }
		public static string PubsubToken { get; private set; }
		public static string ChannelName { get; private set; }
		public static string ChannelId { get; private set; }
		public static string ClientId { get; private set; }
		public TimeSpan StreamUptime { get { return DateTime.Now - streamStartTime; } }

		public Bot()
		{
			initTimer = new Stopwatch();
			initTimer.Start();

			LoadEnvVars();

			AHK = AutoHotkeyEngine.Instance;
			API = API.Instance;
			AudioPlayer = AudioPlayer.Instance;
			TwitchHandler = TwitchHandler.Instance;

			// Subscriptions
			TwitchHandler.OnConnectionChange += OnTwitchConnectionChange;
			TwitchHandler.OnStreamStateChange += OnStreamStateChange;
		}

		#region Public Methods

		public static async Task OnBotClose()
		{
			TwitchHandler.SendMessage("/me Gracefully enters the barn and goes to sleep.");

			if (OBSHandler != null)
				OBSHandler.Disconnect();

			if (RewardHandler != null)
				await RewardHandler.DisableCustomRewards();
		}

		/// <summary>
		/// Appends text to the chat log file
		/// </summary>
		/// <param name="text">The text to append</param>
		public void AppendToLogFile(string text)
		{
			// If file exists
			if (File.Exists(streamLogFullPath))
			{
				using (StreamWriter sw = File.AppendText(streamLogFullPath))
				{
					sw.WriteLine($"[{StreamUptime.ToString(@"hh\:mm\:ss")}] {text}");
				}
			}
		}

		/// <summary>
		/// Replaces a line of text in the chat log file
		/// </summary>
		/// <param name="startOfLineText">The start of the text in the line to replace, or the whole line of text</param>
		/// <param name="newText">The text to replace it with</param>
		public void ReplaceLineInFile(string startOfLineText, string newText)
		{
			// If file exists
			if (File.Exists(streamLogFullPath))
			{
				string[] lines = File.ReadAllLines(streamLogFullPath);
				string lineToReplace = Array.Find(lines, x => x.StartsWith(startOfLineText));
				int indexToReplace = Array.IndexOf(lines, lineToReplace);

				// If line found
				if (indexToReplace >= 0)
				{
					lines[indexToReplace] = newText;
					File.WriteAllLines(streamLogFullPath, lines);
				}
			}
		}

		#endregion

		/// <summary>
		/// Runs when the bot has connected to Twitch and has joined my channel
		/// </summary>
		/// <param name="connectedToPubSub">True if the PubSub connection was successful, false otherwise</param>
		private async void OnTwitchConnectionChange(bool connected)
		{
			IsInChannel = connected;

			if (IsInChannel && !hasStarted)
			{
				TwitchHandler.SendMessage("/me Rushes out of barn and violently tackles everyone.");

				MessageHandler = MessageHandler.Instance;
				RewardHandler = RewardHandler.Instance;

				// Connect to OBS
				OBSHandler.ConnectionArgs obsConnectArgs = new()
				{
					Host = "localhost",
					Port = 4455,
					Password = Utility.LoadEnvVar("OBS_WEBSOCKET_PASS")
				};
				OBSHandler = OBSHandler.Instance;
				connectedToOBS = OBSHandler.Connect(obsConnectArgs).Wait(300);

				if (AmILive || TestLiveStuff)
					await OnStreamStateChange(true);

				initTimer.Stop();
				Debug.Log($"[Initialization took {initTimer.ElapsedMilliseconds}ms]", false);

				OutputStatus();

				Debug.Log("\nEvent log:", false);

				hasStarted = true;
			}
		}

		private async Task OnStreamStateChange(bool started)
		{
			AmILive = started;

			// If stream started, or if testing custom channel point rewards
			if (started)
			{
				streamStartTime = DateTime.Now;
				await RewardHandler.CreateCustomRewards();

				// Only create log file if I'm really live, not when testing custom channel point rewards
				if (!TestLiveStuff)
					CreateLogFile();
			}

			// If stream ended
			if (!started)
			{
				TimeSpan duration = StreamUptime;
				string durationStr = $"{duration.Hours} hours {duration.Minutes} minutes {duration.Seconds} seconds";

				ReplaceLineInFile("Ended at", "Ended at: " + DateTime.Now.ToString("dddd, MMMM dd, yyyy, h:mm:ss tt"));
				ReplaceLineInFile("Duration", $"Duration: {durationStr}");
			}
		}

		private void OutputStatus()
		{
			string[] status = new string[]
			{
				"Connected to Twitch: ",
				"Connected to OBS: ",
				"Joined channel: "
			};

			status[0] += IsInChannel ? '\u2713' : '\u00d7';
			status[1] += connectedToOBS ? '\u2713' : '\u00d7';
			status[2] += IsInChannel ? '\u2713' : '\u00d7';

			foreach (string str in status)
				Debug.Log(str, false, str.EndsWith('\u2713') ? ConsoleColor.Green : ConsoleColor.Red, false);
		}

		#region Private Methods

		/// <summary>
		/// Loads environment variables from the .env file.
		/// </summary>
		private void LoadEnvVars()
		{
			HowardToken = ConfigurationManager.AppSettings["HOWARD_TOKEN"];
			PubsubToken = ConfigurationManager.AppSettings["PUBSUB_TOKEN"];
			ChannelName = ConfigurationManager.AppSettings["CHANNEL_NAME"];
			ChannelId = ConfigurationManager.AppSettings["CHANNEL_ID"];
			ClientId = ConfigurationManager.AppSettings["CLIENT_ID"];
			streamLogsDir = ConfigurationManager.AppSettings["CHAT_LOG_DIRECTORY"];
		}

		/// <summary>
		/// Creates stream log file
		/// </summary>
		private async void CreateLogFile()
		{
			var response = await API.GetChannelInfo(ChannelId);
			string timestamp = streamStartTime.ToString("MM/dd/yyyy HH:mm").Replace('/', '-').Replace(':', '-').Replace(' ', '_');
			streamLogFileName = $"{timestamp}.txt";
			streamLogFullPath = $"{streamLogsDir}\\{streamLogFileName}";

			using (StreamWriter sw = File.CreateText(streamLogFullPath))
			{
				sw.WriteLine($"Title: {response.Title}");
				sw.WriteLine($"Game: {response.GameName}");
				sw.WriteLine($"Started at: {streamStartTime.ToString("dddd, MMMM dd, yyyy, h:mm:ss tt")}");
				sw.WriteLine("Ended at: TBD");
				sw.WriteLine("Duration: TBD\n"); ;
				sw.WriteLine("Messages sent: 0");
				sw.WriteLine("Unique chatters: 0");
				sw.WriteLine("Bot commands used: 0\n");
				sw.WriteLine("Channel point redemptions: 0");
				sw.WriteLine("Channel points spent: 0\n");
				sw.WriteLine("----------------------------------------------------------------------------------------------------\n");
			}

			Debug.Log($"Created chat log '{streamLogFileName}'", true, ConsoleColor.Cyan);
		}

		#endregion
	}
}
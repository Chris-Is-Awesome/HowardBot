using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using AutoHotkey.Interop;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace HowardBot
{
	class Bot : Singleton<Bot>
	{
		private static DateTime streamStartTime;

		private readonly API api;
		private readonly AudioPlayer audioPlayer;
		private readonly MessageHandler messageHandler;
		private readonly RewardHandler rewardHandler;
		private readonly Stopwatch timer;

		private bool hasCreatedLogFile;
		private bool hasCreatedCustomRewards;
		private string chatLogsDir;
		private string chatLogFileName;
		private string fullChatLogPath;

		public static AutoHotkeyEngine AHK { get; private set; }
		public static TwitchPubSub PubSubClient { get; private set; }
		public static TwitchClient TwitchClient { get; private set; }
		public static string HowardToken { get; private set; }
		public static string PubsubToken { get; private set; }
		public static string ChannelName { get; private set; }
		public static string ChannelId { get; private set; }
		public static string ClientId { get; private set; }
		public static bool AmILive { get; private set; } = false; // Set to true to test reward redemptions
		public static TimeSpan StreamUptime
		{
			get
			{
				return DateTime.Now - streamStartTime;
			}
		}

		// Testing
		public static bool UsingChatLog { get; private set; } = false;
		private bool UsingCustomRewards { get; set; } = false;

		public Bot()
		{
			// Start timer
			timer = new Stopwatch();
			timer.Start();

			// Load env vars
			LoadEnvVars();

			// Initialize Twitch client
			ConnectionCredentials credentials = new ConnectionCredentials("The_Goat_Howard", HowardToken);
			var clientOptions = new ClientOptions
			{
				MessagesAllowedInPeriod = 750,
				ThrottlingPeriod = TimeSpan.FromSeconds(30)
			};
			WebSocketClient customClient = new WebSocketClient(clientOptions);
			TwitchClient = new TwitchClient(customClient);
			TwitchClient.Initialize(credentials, "ChrisIsAwesome");

			// Initialize PubSub client
			PubSubClient = new TwitchPubSub();

			// Base event subscriptions
			TwitchClient.OnConnected += OnConnected;
			TwitchClient.OnJoinedChannel += OnJoinedChannel;
			TwitchClient.OnRaidNotification += OnRaid;
			PubSubClient.OnPubSubServiceConnected += OnPubSubConnected;
			PubSubClient.OnListenResponse += OnListenResponse;
			PubSubClient.OnStreamUp += OnStreamStarted;
			PubSubClient.OnStreamDown += OnStreamEnded;

			PubSubClient.ListenToVideoPlayback(ChannelId);

			// Initialize the rest
			api = API.Instance;
			messageHandler = MessageHandler.Instance;
			rewardHandler = RewardHandler.Instance;
			audioPlayer = AudioPlayer.Instance;
			AHK = AutoHotkeyEngine.Instance;

			// Connect
			TwitchClient.Connect();
			PubSubClient.Connect();
		}

		#region Public Methods

		/// <summary>
		/// Sends a message to chat as the bot.
		/// </summary>
		/// <param name="message">The text to send</param>
		public static void SendMessage(string message)
		{
			TwitchClient.SendMessage(ChannelName, message);
		}

		/// <summary>
		/// Replies to a user in chat as the bot.
		/// </summary>
		/// <param name="messageId">The ID of the message to reply to</param>
		/// <param name="message">The text to send</param>
		public static void SendReply(string messageId, string message)
		{
			TwitchClient.SendReply(ChannelName, messageId, message);
		}

		/// <summary>
		/// Appends text to the chat log file
		/// </summary>
		/// <param name="text">The text to append</param>
		public void AppendToLogFile(string text)
		{
			// If file exists
			if (File.Exists(fullChatLogPath))
			{
				using (StreamWriter sw = File.AppendText(fullChatLogPath))
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
			if (File.Exists(fullChatLogPath))
			{
				string[] lines = File.ReadAllLines(fullChatLogPath);
				string lineToReplace = Array.Find(lines, x => x.StartsWith(startOfLineText));
				int indexToReplace = Array.IndexOf(lines, lineToReplace);

				// If line found
				if (indexToReplace >= 0)
				{
					lines[indexToReplace] = newText;
					File.WriteAllLines(fullChatLogPath, lines);
				}
			}
		}

		#endregion

		#region Event Methods

		// When bot is connected to Twitch
		private void OnConnected(object sender, OnConnectedArgs e)
		{
			Debug.Log($">> Connected to Twitch!");

			DoWhenStreamStartsOrBotConnects();
		}

		// When bot is connected to PubSub service
		private void OnPubSubConnected(object sender, EventArgs e)
		{
			Debug.Log(">> Connected to PubSub!");
			PubSubClient.SendTopics(PubsubToken);
		}

		// When bot tries to connect to a PubSub topic
		private void OnListenResponse(object sender, OnListenResponseArgs e)
		{
			if (!e.Successful)
				Debug.LogError($">> Failed to listen to <{e.Topic}> ({e.Response.Error})");
			else
				Debug.Log($">> Listening to PubSub topic <{e.Topic}>...");
		}

		// When bot has connected to my channel and is ready for action
		private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
		{
			// Stop timer
			timer.Stop();

			// Howard is ready for action!
			Debug.Log($">> Joined channel {e.Channel} as {e.BotUsername}!\n[Connection took {timer.Elapsed.Milliseconds}ms]");
			Debug.Log("Event log:", false);
			SendMessage("/me Rushes out of barn and violently tackles everyone.");

			CheckIfLive();
		}

		// When my stream is started
		private void OnStreamStarted(object sender, OnStreamUpArgs e)
		{
			AmILive = true;
			DoWhenStreamStartsOrBotConnects();
		}

		// When my stream has ended
		private void OnStreamEnded(object sender, OnStreamDownArgs e)
		{
			AmILive = false;

			TimeSpan duration = StreamUptime;
			string durationStr = $"{duration.Hours} hours {duration.Minutes} minutes {duration.Seconds} seconds";

			ReplaceLineInFile("Ended at", "Ended at: " + DateTime.Now.ToString("dddd, MMMM dd, yyyy, h:mm:ss tt"));
			ReplaceLineInFile("Duration", $"Duration: {durationStr}");
		}

		// When someone raids my channel
		private void OnRaid(object sender, OnRaidNotificationArgs e)
		{
			SendMessage($"{e.RaidNotification.DisplayName} brought {e.RaidNotification.MsgParamViewerCount} goats into the barn! 🐐");
			messageHandler.RunCommand("shoutout", new string[] { e.RaidNotification.UserId });
			AppendToLogFile($"[RAID] {e.RaidNotification.DisplayName} raided with {e.RaidNotification.MsgParamViewerCount} viewers!");
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Recheck if I'm live in case where PubSub OnStreamStarted event doesn't run (eg. in case of bot crash during stream)
		/// </summary>
		private async void CheckIfLive()
		{
			await Utility.WaitForSeconds(10);

			var response = await api.GetStreamForUser(ChannelId);

			if (response != null)
			{
				AmILive = true;
				DoWhenStreamStartsOrBotConnects();
			}

			CheckIfLive();
		}

		/// <summary>
		/// All the things to do when straem starts or bot connects
		/// </summary>
		private async void DoWhenStreamStartsOrBotConnects()
		{
			if (!hasCreatedLogFile && (AmILive || (!AmILive && UsingChatLog)))
				CreateLogFile();

			if (!hasCreatedCustomRewards && (AmILive || (!AmILive && UsingCustomRewards)))
			{
				await rewardHandler.CreateCustomRewards();
				hasCreatedCustomRewards = true;
			}

			if (AmILive && !UsingChatLog)
				UsingChatLog = true;
		}

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
			chatLogsDir = ConfigurationManager.AppSettings["CHAT_LOG_DIRECTORY"];
		}

		/// <summary>
		/// Creates stream log file
		/// </summary>
		private async void CreateLogFile()
		{
			var response = await api.GetChannelInfo(ChannelId);
			streamStartTime = DateTime.Now;
			string timestamp = streamStartTime.ToString("MM/dd/yyyy HH:mm").Replace('/', '-').Replace(':', '-').Replace(' ', '_');
			chatLogFileName = $"{timestamp}.txt";
			fullChatLogPath = $"{chatLogsDir}\\{chatLogFileName}";

			using (StreamWriter sw = File.CreateText(fullChatLogPath))
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

			hasCreatedLogFile = true;
			Debug.Log($"Created chat log '{chatLogFileName}'");
		}

		#endregion
	}
}
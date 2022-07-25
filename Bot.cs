using AutoHotkey.Interop;
using System;
using System.Diagnostics;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace HowardBot
{
	class Bot
	{
		private readonly API api;
		private readonly MessageHandler messageHandler;
		private readonly Stopwatch timer;

		public static AutoHotkeyEngine AHK { get; private set; }
		public static TwitchPubSub PubSubClient { get; private set; }
		public static TwitchClient TwitchClient { get; private set; }
		public static string HowardToken { get; private set; }
		public static string PubsubToken { get; private set; }
		public static string ChannelName { get; private set; }
		public static string ChannelId { get; private set; }
		public static string ClientId { get; private set; }

		public Bot()
		{
			// Start timer
			timer = new Stopwatch();
			timer.Start();

			// Load env vars
			LoadEnvVars();

			// Initialize API
			api = API.Instance;

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

			// Initialize event handlers
			messageHandler = new(TwitchClient);
			RewardHandler rewardHandler = new();

			// Connect
			TwitchClient.Connect();
			PubSubClient.Connect();

			// Initialize AHK
			AHK = AutoHotkeyEngine.Instance;
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

		#endregion

		#region Event Methods

		// When bot is connected to Twitch
		private void OnConnected(object sender, OnConnectedArgs e)
		{
			Debug.Log($">> Connected to Twitch!");
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
		}

		// When someone raids my channel
		private void OnRaid(object sender, OnRaidNotificationArgs e)
		{
			SendMessage($"{e.RaidNotification.DisplayName} brought {e.RaidNotification.MsgParamViewerCount} goats into the barn! 🐐");
			messageHandler.RunCommand("shoutout", new string[] { e.RaidNotification.UserId });
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Loads environment variables from the .env file.
		/// </summary>
		private void LoadEnvVars()
		{
			DotNetEnv.Env.Load();
			HowardToken = Environment.GetEnvironmentVariable("HOWARD_TOKEN");
			PubsubToken = Environment.GetEnvironmentVariable("PUBSUB_TOKEN");
			ChannelName = Environment.GetEnvironmentVariable("CHANNEL_NAME");
			ChannelId = Environment.GetEnvironmentVariable("CHANNEL_ID");
			ClientId = Environment.GetEnvironmentVariable("CLIENT_ID");
		}

		#endregion
	}
}
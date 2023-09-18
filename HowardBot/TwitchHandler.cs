using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub;

namespace HowardBot
{
	class TwitchHandler : Singleton<TwitchHandler>
	{
		private static TwitchClient twitch;
		private readonly TwitchPubSub pubsub;
		private readonly List<string> pubsubTopics = new();
		private readonly string channelId;

		public delegate void ConnectionFunc(bool connected);
		public delegate Task StreamFunc(bool started);

		public static event ConnectionFunc OnConnectionChange;
		public static event StreamFunc OnStreamStateChange;

		public static TwitchClient TwitchClient { get { return twitch; } }
		public TwitchPubSub PubSubClient { get { return pubsub; } }

		public TwitchHandler()
		{
			channelId = Utility.LoadEnvVar("CHANNEL_ID");

			// Initialize Twitch client
			ConnectionCredentials credentials = new("The_Goat_Howard", Utility.LoadEnvVar("HOWARD_TOKEN"));
			ClientOptions clientOptions = new()
			{
				MessagesAllowedInPeriod = 750,
				ThrottlingPeriod = TimeSpan.FromSeconds(30)
			};
			WebSocketClient client = new(clientOptions);
			twitch = new TwitchClient(client);
			twitch.Initialize(credentials, "ChrisIsAwesome");

			// Initialize PubSub client
			pubsub = new TwitchPubSub();

			// Event subscriptions
			twitch.OnConnectionError += OnConnectionError;
			twitch.OnDisconnected += OnDisconnected;
			twitch.OnJoinedChannel += OnJoinedChannel;
			twitch.OnRaidNotification += OnRaidNotification;
			pubsub.OnPubSubServiceConnected += OnPubSubConnected;
			pubsub.OnListenResponse += OnPubSubTopicListen;
			pubsub.OnStreamUp += OnStreamStarted;
			pubsub.OnStreamDown += OnStreamEnded;

			// Connect
			twitch.Connect();
			pubsub.Connect();

			// Listen to PubSub topics
			pubsub.ListenToVideoPlayback(channelId);
			pubsub.ListenToChannelPoints(channelId);
		}

		/// <summary>
		/// Sends a message in chat as the bot.
		/// </summary>
		/// <param name="message">The message to send</param>
		public static void SendMessage(string message)
		{
			if (Bot.IsInChannel)
				twitch.SendMessage("chrisisawesome", message);
		}

		/// <summary>
		/// Replies to a message in chat as the bot.
		/// </summary>
		/// <param name="replyToId">The ID of the message to reply to</param>
		/// <param name="message">The message to send</param>
		public static void SendReply(string replyToId, string message)
		{
			if (Bot.IsInChannel)
			{
				twitch.SendReply("chrisisawesome", replyToId, message);
				Debug.Log(message);
			}
		}

		private void OnConnectionError(object sender, TwitchLib.Client.Events.OnConnectionErrorArgs e)
		{
			Debug.LogError($"Failed to connect to Twitch — {e.Error.Message}", false);
		}

		private void OnDisconnected(object sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
		{
			OnConnectionChange?.Invoke(false);
		}

		private async void OnJoinedChannel(object sender, TwitchLib.Client.Events.OnJoinedChannelArgs e)
		{
			OnConnectionChange?.Invoke(true);

			// Check if it connected mid-stream
			var response = await Bot.API.GetStreamForUser(channelId);

			// If live
			if (response != null)
				OnStreamStateChange?.Invoke(true);
		}

		private void OnPubSubConnected(object sender, EventArgs e)
		{
			pubsub.SendTopics(Utility.LoadEnvVar("PUBSUB_TOKEN"));
		}

		private void OnPubSubTopicListen(object sender, TwitchLib.PubSub.Events.OnListenResponseArgs e)
		{
			if (e.Successful)
				pubsubTopics.Add(e.Topic);
			else
				Debug.LogWarning($"[PubSub] Failed to listen to PubSub topic '{e.Topic}'\n{e.Response.Error}");
		}

		private void OnStreamStarted(object sender, TwitchLib.PubSub.Events.OnStreamUpArgs e)
		{
			OnStreamStateChange?.Invoke(true);
		}

		private void OnStreamEnded(object sender, TwitchLib.PubSub.Events.OnStreamDownArgs e)
		{
			OnStreamStateChange?.Invoke(false);
		}

		private void OnRaidNotification(object sender, TwitchLib.Client.Events.OnRaidNotificationArgs e)
		{
			SendMessage($"{e.RaidNotification.DisplayName} brought {e.RaidNotification.MsgParamViewerCount} goats into the barn! 🐐");
			Bot.MessageHandler.RunCommand("shoutout", new string[] { e.RaidNotification.UserId });
			Bot.Instance.AppendToLogFile($"[RAID] {e.RaidNotification.DisplayName} raided with {e.RaidNotification.MsgParamViewerCount} viewers!");
		}
	}
}
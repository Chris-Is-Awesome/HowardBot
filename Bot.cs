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
		readonly AutoHotkeyEngine ahk;
		static TwitchClient twitchClient;
		readonly TwitchPubSub pubSubClient;
		public static string howardToken;
		public static string pubsubToken;
		public static string channelName;
		public static string channelId;
		public static string clientId;
		private readonly Stopwatch timer;

		public Bot()
		{
			// Start timer
			timer = new Stopwatch();
			timer.Start();

			// Load env vars
			DotNetEnv.Env.Load();
			howardToken = Environment.GetEnvironmentVariable("HOWARD_TOKEN");
			pubsubToken = Environment.GetEnvironmentVariable("PUBSUB_TOKEN");
			channelName = Environment.GetEnvironmentVariable("CHANNEL_NAME");
			channelId = Environment.GetEnvironmentVariable("CHANNEL_ID");
			clientId = Environment.GetEnvironmentVariable("CLIENT_ID");

			// Initialize AHK
			ahk = AutoHotkeyEngine.Instance;

			// Initialize Twitch client
			ConnectionCredentials credentials = new ConnectionCredentials("The_Goat_Howard", howardToken);
			var clientOptions = new ClientOptions
			{
				MessagesAllowedInPeriod = 750,
				ThrottlingPeriod = TimeSpan.FromSeconds(30)
			};
			WebSocketClient customClient = new WebSocketClient(clientOptions);
			twitchClient = new TwitchClient(customClient);
			twitchClient.Initialize(credentials, "ChrisIsAwesome");

			// Initialize PubSub client
			pubSubClient = new TwitchPubSub();

			// Base event subscriptions
			twitchClient.OnConnected += OnConnected;
			twitchClient.OnJoinedChannel += OnJoinedChannel;
			pubSubClient.OnPubSubServiceConnected += OnPubSubConnected;
			pubSubClient.OnListenResponse += OnListenResponse;

			// Initialize event handlers
			MessageHandler messageHandler = new(twitchClient);
			RewardHandler rewardHandler = new(pubSubClient, ahk);

			// Connect
			twitchClient.Connect();
			pubSubClient.Connect();
		}

		private void OnConnected(object sender, OnConnectedArgs e)
		{
			Debug.Log($">> Connected to Twitch!");
		}


		private void OnPubSubConnected(object sender, EventArgs e)
		{
			Debug.Log(">> Connected to PubSub!");
			pubSubClient.SendTopics(pubsubToken);
		}

		private void OnListenResponse(object sender, OnListenResponseArgs e)
		{
			if (!e.Successful)
				Debug.LogError($">> Failed to listen to <{e.Topic}> ({e.Response.Error})");
			else
				Debug.Log($">> Listening to PubSub topic <{e.Topic}>...");
		}

		private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
		{
			// Stop timer
			timer.Stop();

			// Howard is ready for action!
			Debug.Log($">> Joined channel ChrisIsAwesome!\n[Connection took {timer.Elapsed.Milliseconds}ms]");
			Debug.Log("Event log:", false);
			SendMessage("/me Rushes out of barn and violently tackles everyone.");
		}

		public static void SendMessage(string message)
		{
			twitchClient.SendMessage(channelName, message);
		}
	}
}
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

namespace HowardPlays
{
	class Bot
	{
		AutoHotkeyEngine ahk;
		static TwitchClient twitchClient;
		TwitchPubSub pubSubClient;
		public static string howardToken;
		public static string pubsubToken;
		public static string channelName;
		public static string channelId;
		public static string clientId;
		Stopwatch timer = new Stopwatch();

		public Bot()
		{
			// Start timer
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
			ConnectionCredentials credentials = new ConnectionCredentials("The_goat_howard", howardToken);
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

		public static string GetTimestamp()
		{
			return $"<< [{ DateTime.Now.ToShortTimeString()}]";
		}

		private void OnListenResponse(object sender, OnListenResponseArgs e)
		{
			if (!e.Successful)
			{
				Console.WriteLine($">> Failed to listen to <{e.Topic}> ({e.Response.Error})");
			}
			else
			{
				Console.WriteLine($">> Listening to PubSub topic <{e.Topic}>...");
			}
		}

		private void OnPubSubConnected(object sender, EventArgs e)
		{
			Console.WriteLine(">> Connected to PubSub!");
			pubSubClient.SendTopics(pubsubToken);
		}

		private void OnConnected(object sender, OnConnectedArgs e)
		{
			Console.WriteLine($">> Connected to Twitch!\n");
		}

		private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
		{
			// Stop timer
			timer.Stop();

			Console.WriteLine($"\n>> Joined channel ChrisIsAwesome!");
			Console.WriteLine($"[Connection took {timer.Elapsed.Milliseconds}ms]\n");
			Console.WriteLine("Event log:\n");
			SendMessage("/me Tackles wyrm and herds him into barn");
		}

		public static void SendMessage(string message)
		{
			twitchClient.SendMessage(channelName, message);
		}
	}
}
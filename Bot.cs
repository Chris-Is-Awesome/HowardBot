using System;
using System.Collections.Generic;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace HowardPlays
{
	class Bot
	{
		TwitchClient client;

		public Bot()
		{
			// Load env vars
			DotNetEnv.Env.Load();

			// Initialize client
			ConnectionCredentials credentials = new ConnectionCredentials("The_goat_howard", Environment.GetEnvironmentVariable("ACCESS_TOKEN"));
			var clientOptions = new ClientOptions
			{
				MessagesAllowedInPeriod = 750,
				ThrottlingPeriod = TimeSpan.FromSeconds(30)
			};
			WebSocketClient customClient = new WebSocketClient(clientOptions);
			client = new TwitchClient(customClient);
			client.Initialize(credentials, "ChrisIsAwesome");

			// Event subscriptions
			client.OnLog += OnLog;
			client.OnConnected += OnConnected;
			client.OnJoinedChannel += OnJoinedChannel;
			client.OnMessageReceived += OnMessageReceived;

			// Connect
			client.Connect();
		}

		private void OnLog(object sender, OnLogArgs e)
		{
			//Console.WriteLine(e.Data);
		}

		private void OnConnected(object sender, OnConnectedArgs e)
		{
			Console.WriteLine($"Connected to Twitch!");
		}

		private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
		{
			Console.WriteLine($"Joined channel ChrisIsAwesome!");
			//client.SendMessage(e.Channel, "/me Tackles wyrm and herds him into barn");
		}

		private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
		{
			Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] {e.ChatMessage.DisplayName}: {e.ChatMessage.Message}");
		}
	}
}
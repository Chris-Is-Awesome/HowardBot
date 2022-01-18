using System;
using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace HowardPlays
{
	class MessageHandler
	{
		TwitchClient client;

		public MessageHandler(TwitchClient twitchClient)
		{
			client = twitchClient;
			client.OnMessageReceived += OnMessageReceived;
		}

		private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
		{
			Console.WriteLine($"{Bot.GetTimestamp()} [Chat] {e.ChatMessage.DisplayName}: {e.ChatMessage.Message}");
		}
	}
}
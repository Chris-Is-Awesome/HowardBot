using AutoHotkey.Interop;
using System;
using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace HowardPlays
{
	class MessageHandler
	{
		AutoHotkeyEngine ahk;
		TwitchClient client;

		public MessageHandler(TwitchClient twitchClient)
		{
			ahk = AutoHotkeyEngine.Instance;
			client = twitchClient;
			client.OnMessageReceived += OnMessageReceived;
		}

		private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
		{
			Console.WriteLine($"{Bot.GetTimestamp()} {e.ChatMessage.DisplayName}: {e.ChatMessage.Message}");
			ahk.ExecRaw("Send {A}");
		}
	}
}
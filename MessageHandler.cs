using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace HowardPlays
{
	class MessageHandler
	{
		private readonly TwitchClient client;

		public MessageHandler(TwitchClient twitchClient)
		{
			client = twitchClient;
			client.OnMessageReceived += OnMessageReceived;
		}

		private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
		{
			Debug.Log($"[Chat] {e.ChatMessage.DisplayName}: {e.ChatMessage.Message}");
		}
	}
}
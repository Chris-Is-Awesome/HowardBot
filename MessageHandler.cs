using HowardPlays.Commands;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace HowardPlays
{
	class MessageHandler
	{
		public MessageHandler(TwitchClient twitchClient)
		{
			client = twitchClient;
			client.OnMessageReceived += OnMessageReceived;
		}

		private class CommandInfo
		{
			public string name;
			public ICommand command;
			public bool sendMessage;

			public CommandInfo(string name, ICommand command, bool sendMessage = true)
			{
				this.name = name;
				this.command = command;
				this.sendMessage = sendMessage;
			}
		}

		private readonly TwitchClient client;
		private delegate void CommandFunc(string[] args);
		private readonly char prefix = '!';
		private readonly List<CommandInfo> commands = new List<CommandInfo>();

		private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
		{
			ChatMessage chat = e.ChatMessage;
			Debug.Log($"[Chat] {chat.DisplayName}: {chat.Message}");

			// Handle commands
			if (TryParseCommand(chat, out CommandInfo commandInfo, out string[] args))
			{
				Debug.Log($"[Chat - Command] Command '{commandInfo.name}' executed by '{chat.DisplayName}'.");

				if (commandInfo.sendMessage)
					Bot.SendMessage(commandInfo.command.Run(args, chat.DisplayName));
				else
					commandInfo.command.Run(args, chat.DisplayName);
			}
		}

		private bool TryParseCommand(ChatMessage chat, out CommandInfo command, out string[] args)
		{
			string message = chat.Message;

			// If starts with prefix
			if (message.StartsWith(prefix))
			{
				// Split words to get command name and args separately
				string[] splitMessage = message.Substring(1, message.Length - 1).Split(' ');
				string commandName = splitMessage[0].ToLower();
				Bot.SendMessage(commandName);
				command = commands.Find(x => x.name == commandName);

				// If is a valid command
				if (command != null)
				{
					args = splitMessage.Skip(1).ToArray();
					return true;
				}
				// If invalid command
				else
					Bot.SendMessage($"@{chat.DisplayName} No command named '{commandName}' was found. Either you made a typo or Chris is dumber than a Stupid Bee.");
			}

			command = null;
			args = null;
			return false;
		}
	}
}
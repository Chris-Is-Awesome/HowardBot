using HowardBot.Commands;
using System.Collections.Generic;
using System.Linq;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace HowardBot
{
	class MessageHandler
	{
		public MessageHandler(TwitchClient twitchClient)
		{
			client = twitchClient;
			client.OnMessageReceived += OnMessageReceived;

			// Define & initialize commands
			commands = new List<CommandInfo>()
			{
				{ new CommandInfo("whoop", new WhoopCommand(), new string[] { "w" }) },
				{ new CommandInfo("bff", new BffCommand()) },
				{ new CommandInfo("trivia", new TriviaCommand()) }
			};
		}

		private readonly TwitchClient client;
		private readonly List<CommandInfo> commands;
		private readonly char prefix = '!';

		private delegate void CommandFunc(string[] args);

		public void RunCommand(string commandName, string[] args = null)
		{
			if (TryParseCommand($"!{commandName}", out CommandInfo commandInfo))
				Bot.SendMessage(commandInfo.command.Run(args));
		}

		private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
		{
			ChatMessage chat = e.ChatMessage;
			Debug.Log($"[Chat] {chat.DisplayName}: {chat.Message}");

			// Handle commands
			if (TryParseCommand(chat, out CommandInfo commandInfo, out string[] args))
			{
				Debug.Log($"[Chat - Command] Command '{commandInfo.name}' executed by '{chat.DisplayName}'.");

				if (commandInfo.sendMessage)
					if (commandInfo.reply)
						Bot.SendReply(chat.Id, commandInfo.command.Run(args));
					else
						Bot.SendMessage(commandInfo.command.Run(args));
				else
					commandInfo.command.Run(args);
			}
		}

		private bool TryParseCommand(ChatMessage chat, out CommandInfo commandInfo, out string[] args)
		{
			string message = chat.Message;

			// If starts with prefix
			if (message.StartsWith(prefix))
			{
				// Split words to get command name and args separately
				string[] splitMessage = message.Substring(1, message.Length - 1).Split(' ');
				string commandName = splitMessage[0].ToLower();
				commandInfo = GetCommandInfo(commandName);

				// If command valid
				if (commandInfo != null)
				{
					args = splitMessage.Skip(1).ToArray();
					return true;
				}
				// If command invalid
				else
					Bot.SendReply(chat.Id, $"No command named '{commandName}' was found. Either you made a typo or Chris is dumber than a Stupid Bee.");
			}

			commandInfo = null;
			args = null;
			return false;

		}

		private bool TryParseCommand(string text, out CommandInfo commandInfo)
		{
			// If starts with prefix
			if (text.StartsWith(prefix))
			{
				// Split words to get command name and args separately
				string[] splitMessage = text.Substring(1, text.Length - 1).Split(' ');
				string commandName = splitMessage[0].ToLower();
				commandInfo = GetCommandInfo(commandName);

				// If command valid
				if (commandInfo != null)
					return true;
				// If command invalid
				else
					Debug.LogError($"Tried to run invalid command {commandName}.");
			}

			commandInfo = null;
			return false;
		}

		private CommandInfo GetCommandInfo(string name)
		{
			return commands.Find(x => x.name == name || (x.aliases != null && x.aliases.Contains(name)));
		}

		private class CommandInfo
		{
			public string name;
			public ICommand command;
			public string[] aliases;
			public bool sendMessage;
			public bool reply;

			public CommandInfo(string name, ICommand command, string[] aliases = null, bool reply = true, bool sendMessage = true)
			{
				this.name = name;
				this.command = command;
				this.aliases = aliases;
				this.sendMessage = sendMessage;
				this.reply = reply;
			}
		}
	}
}
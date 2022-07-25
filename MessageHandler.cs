using HowardBot.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
				{ new CommandInfo("trivia", new TriviaCommand()) },
				{ new CommandInfo("shoutout", new ShoutoutCommand(), new string[] { "so" }, true) }
			};
		}

		private static MessageHandler _instance;

		private readonly TwitchClient client;
		private readonly List<CommandInfo> commands;
		private readonly char prefix = '!';

		public static MessageHandler Instance
		{
			get
			{
				if (_instance == null)
					_instance = new MessageHandler(Bot.TwitchClient);

				return _instance;
			}
		}

		private delegate void CommandFunc(string[] args);

		/// <summary>
		/// Manually run a bot command by name.
		/// </summary>
		/// <param name="commandName">The name of the command or an alias</param>
		/// <param name="args">The arguments for the command</param>
		public async void RunCommand(string commandName, string[] args = null)
		{
			if (TryParseCommand($"!{commandName}", out CommandInfo commandInfo))
				Bot.SendMessage(await DoRunCommand(commandInfo, args));
		}

		// When a message is sent to chat
		private async void OnMessageReceived(object sender, OnMessageReceivedArgs e)
		{
			ChatMessage chat = e.ChatMessage;
			Debug.Log($"[Chat] {chat.DisplayName}: {chat.Message}");

			// Handle commands
			if (TryParseCommand(chat, out CommandInfo commandInfo, out string[] args))
			{
				Debug.Log($"[Chat - Command] Command '{commandInfo.name}' executed by '{chat.DisplayName}'.");

				if (commandInfo.sendMessage)
					if (commandInfo.reply)
						Bot.SendReply(chat.Id, await DoRunCommand(commandInfo, args));
					else
						Bot.SendMessage(await DoRunCommand(commandInfo, args));
				else
					await DoRunCommand(commandInfo, args);
			}
		}

		/// <summary>
		/// Parses <paramref name="chat"/> to determine if it represents a valid command.
		/// </summary>
		/// <param name="chat">The ChatMessage to parse</param>
		/// <param name="commandInfo">The parsed CommandInfo object</param>
		/// <param name="args">The parsed string array of arguments</param>
		/// <returns>[bool] True if the command is valid, false otherwise.</returns>
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

		/// <summary>
		/// Parses <paramref name="text"/> to determine if it represents a valid command.
		/// </summary>
		/// <param name="text">The text to parse</param>
		/// <param name="commandInfo">The parsed CommandInfo object</param>
		/// <returns>[bool] True if the command is valid, false otherwise.</returns>
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

		/// <summary>
		/// Gets the CommandInfo object from the command's name.
		/// </summary>
		/// <param name="name">The name of the command</param>
		/// <returns>[CommandInfo] The CommandInfo object if it exists, null otherwise.</returns>
		private CommandInfo GetCommandInfo(string name)
		{
			return commands.Find(x => x.name == name || (x.aliases != null && x.aliases.Contains(name)));
		}

		/// <summary>
		/// Runs the appropriate delegate for the command.
		/// </summary>
		/// <param name="commandInfo">The CommandInfo for the command</param>
		/// <param name="args">The arguments for the command</param>
		/// <returns>[string] The text returned from the command delegate.</returns>
		private async Task<string> DoRunCommand(CommandInfo commandInfo, string[] args)
		{
			if (commandInfo.async)
				return await commandInfo.command.RunAsync(args);
			else
				return commandInfo.command.Run(args);
		}

		private class CommandInfo
		{
			public string name;
			public Command command;
			public string[] aliases;
			public bool async;
			public bool sendMessage;
			public bool reply;

			public CommandInfo(string name, Command command, string[] aliases = null, bool async = false, bool reply = true, bool sendMessage = true)
			{
				this.name = name;
				this.command = command;
				this.aliases = aliases;
				this.async = async;
				this.sendMessage = sendMessage;
				this.reply = reply;
			}
		}
	}
}
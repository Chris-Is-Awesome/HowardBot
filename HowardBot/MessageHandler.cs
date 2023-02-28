using HowardBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
			_instance = this;

			client = twitchClient;
			client.OnMessageReceived += OnMessageReceived;

			// Define & initialize commands
			commands = new List<CommandInfo>()
			{
				// Utility
				{ new CommandInfo("commands", new CommandsCommand()) },
				{ new CommandInfo("help", new HelpCommand()) },
				{ new CommandInfo("info", new InfoCommand(), timerCommandsToAlternate: new string[] { "discord", "youtube" }) },

				// Dev
				{ new CommandInfo("stopaudio", new StopAudioCommand(), aliases: new string[] { "stopsongs", "stopsounds" }, isDev: true) },
				{ new CommandInfo("test", new TestCommand(), aliases: new string[] { "t" }, isDev: true) },

				// Self promos
				{ new CommandInfo("discord", new DiscordCommand(), aliases: new string[] { "disc" }, timerCommandsToAlternate: new string[] { "info", "youtube" }) },
				{ new CommandInfo("youtube", new YoutubeCommand(), aliases: new string[] { "yt" }, timerCommandsToAlternate: new string[] { "discord", "info" }) },

				// Useful
				{ new CommandInfo("shoutout", new ShoutoutCommand(), aliases: new string[] { "so" }, async: true) },

				// Fun
				{ new CommandInfo("8ball", new EightBallCommand(), aliases: new string[] { "8b" }, argsCaseSensitive: true) },
				{ new CommandInfo("bff", new BffCommand()) },
				{ new CommandInfo("trivia", new TriviaCommand()) },
				{ new CommandInfo("whoop", new WhoopCommand(), aliases: new string[] { "w" }) },
			};

			// Start timer to handle commands on timers
			timer = new Timer((e) =>
			{
				CheckTimerCommands();
			}, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
		}

		public readonly List<CommandInfo> commands;

		private static MessageHandler _instance;
		private static List<CommandInfo> timerCommands = new List<CommandInfo>();

		private readonly TwitchClient client;
		private readonly Timer timer;
		private readonly char prefix = '!';

		private List<string> uniqueChatters = new List<string>();
		private int messagesThisStream;
		private int commandsThisStream;

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

			// Handle commands
			if (TryParseCommand(chat, out CommandInfo commandInfo, out string[] args))
			{
				if (!commandInfo.isDev || (commandInfo.isDev && chat.UserId == Bot.ChannelId))
				{
					Debug.Log($"[Chat - Command] Command '{commandInfo.name}' executed by '{chat.DisplayName}'.");

					if (commandInfo.sendMessage)
						if (commandInfo.reply)
							Bot.SendReply(chat.Id, await DoRunCommand(commandInfo, args));
						else
							Bot.SendMessage(await DoRunCommand(commandInfo, args));
					else
						await DoRunCommand(commandInfo, args);

					if (Bot.AmILive) LogMessage(chat, commandInfo);
				}
			}
			else
				if (Bot.AmILive) LogMessage(chat, null);
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
				string commandName = GetCommandName(splitMessage[0].ToLower());
				commandInfo = GetCommandInfo(commandName);

				// If command valid
				if (commandInfo != null)
				{
					Command command = commandInfo.command;
					args = splitMessage.Skip(1).ToArray();
					bool justDisabled = false;

					// Lowercase args
					if (!commandInfo.argsCaseSensitive)
						for (int i = 0; i < args.Length; i++)
							args[i] = args[i].ToLower();

					// If enabling or disabling
					if (chat.UserId == Bot.ChannelId && args != null && args.Length > 0)
					{
						// Enable
						if (args[0] == "enable" && !command.Enabled)
						{
							command.Enabled = true;
							Bot.SendReply(chat.Id, $"The command {commandName} has been enabled.");
						}
						// Disable
						else if (args[0] == "disable" && command.Enabled)
						{
							command.Enabled = false;
							justDisabled = true;
							Bot.SendReply(chat.Id, $"The command {commandName} has been disabled.");
						}
					}

					// If command is enabled
					if (command.Enabled)
						return true;
					else if (!justDisabled)
						Bot.SendReply(chat.Id, $"The command '{commandName}' is disabled.");
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
		/// Gets the CommandInfo object from the command's class.
		/// </summary>
		/// <param name="command">The Command class for the command</param>
		/// <returns>[CommandInfo] The CommandInfo object if it exists, null otherwise.</returns>
		private CommandInfo GetCommandInfo(Command command)
		{
			return commands.Find(x => x.command == command);
		}

		/// <summary>
		/// Gets the name of the command. Useful for getting full name of command from one of its aliases.
		/// </summary>
		/// <param name="text">The name or alias of the command</param>
		/// <returns>[string] The name of the command</returns>
		private string GetCommandName(string text)
		{
			return commands.Find(x => x.name == text || x.aliases != null && x.aliases.Contains(text)).name;
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

		/// <summary>
		/// Logs chat messages to a text file
		/// </summary>
		/// <param name="chat">The ChatMessage sent</param>
		private void LogMessage(ChatMessage chat, CommandInfo commandInfo)
		{
			// Update stats
			messagesThisStream++;
			if (commandInfo != null) commandsThisStream++;

			if (!uniqueChatters.Contains(chat.DisplayName))
				uniqueChatters.Add(chat.DisplayName);

			// Log text
			Bot.Instance.ReplaceLineInFile("Messages sent", $"Messages sent: {messagesThisStream}");
			Bot.Instance.ReplaceLineInFile("Unique chatters", $"Unique chatters: {uniqueChatters.Count}");
			Bot.Instance.ReplaceLineInFile("Bot commands used", $"Bot commands used: {commandsThisStream}");
			Bot.Instance.AppendToLogFile($"[Chat] {chat.DisplayName}: {chat.Message}");
			if (commandInfo != null) Bot.Instance.AppendToLogFile($"[Chat - Command] Command '{commandInfo.name}' executed by '{chat.DisplayName}'");

			Debug.Log($"[Chat] {chat.DisplayName}: {chat.Message}");
		}

		/// <summary>
		/// Handles running all timer commands. Runs once per minute.
		/// </summary>
		private async void CheckTimerCommands()
		{
			if (Bot.AmILive)
			{
				foreach (CommandInfo commandInfo in timerCommands)
				{
					double timeSince = (DateTime.Now - commandInfo.timerLastFired).TotalMinutes;

					// Check if timerInterval time has passed since command last ran
					if (timeSince >= commandInfo.timerInterval)
					{
						CommandInfo commandToRun = commandInfo;
						DateTime oldestTime = commandToRun.timerLastFired;

						// Check if command alternates with any and run the one that ran the earliest
						foreach (string commandName in commandInfo.timerCommandsToAlternate)
						{
							CommandInfo altCommand = GetCommandInfo(commandName);

							if (altCommand.timerLastFired < oldestTime)
							{
								commandToRun = altCommand;
								oldestTime = altCommand.timerLastFired;
							}

							altCommand.timerLastFired = DateTime.Now;
						}

						// Run command
						Bot.SendMessage(await DoRunCommand(commandToRun, null));

						// Update timerLastFired to current time
						commandToRun.timerLastFired = DateTime.Now;
					}
				}
			}
		}

		public class CommandInfo
		{
			// Initialization
			public string name;
			public Command command;
			public string[] aliases;
			public bool async;
			public bool argsCaseSensitive;
			public float timerInterval;
			public string[] timerCommandsToAlternate;
			public bool sendMessage;
			public bool reply;
			public bool isDev;

			// Timer
			public DateTime timerLastFired;

			public CommandInfo(string name, Command command, string[] aliases = null, bool async = false, bool argsCaseSensitive = false, float timerInterval = 0, string[] timerCommandsToAlternate = null, bool reply = true, bool sendMessage = true, bool isDev = false)
			{
				this.name = name;
				this.command = command;
				this.aliases = aliases;
				this.async = async;
				this.argsCaseSensitive = argsCaseSensitive;
				this.timerInterval = timerInterval;
				this.timerCommandsToAlternate = timerCommandsToAlternate;
				this.sendMessage = sendMessage;
				this.reply = reply;
				this.isDev = isDev;

				// Add to list of timers
				if (timerInterval > 0)
				{
					timerCommands.Add(this);
					timerLastFired = DateTime.Now;
				}
			}
		}
	}
}
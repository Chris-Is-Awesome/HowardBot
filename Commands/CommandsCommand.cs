using System.Collections.Generic;

namespace HowardBot.Commands
{
	class CommandsCommand : Command
	{
		public CommandsCommand() { }

		public override string Run(string[] args)
		{
			List<MessageHandler.CommandInfo> commands = MessageHandler.Instance.commands;
			string output = "";

			foreach (MessageHandler.CommandInfo command in commands)
			{
				if (!command.isDev)
					output += $"!{command.name} ";
			}

			return output;
		}
	}
}
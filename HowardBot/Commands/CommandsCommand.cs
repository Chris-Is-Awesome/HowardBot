using System.Collections.Generic;

namespace HowardBot.Commands
{
	class CommandsCommand : Command
	{
		private List<MessageHandler.CommandInfo> commands;
		private string output;

		public override string Run(string[] args)
		{
			// Get commands first time
			if (commands == null)
			{
				commands = MessageHandler.Instance.commands;

				foreach (MessageHandler.CommandInfo command in commands)
				{
					if (!command.isDev)
						output += $"!{command.name} ";
				}
			}

			return output;
		}
	}
}
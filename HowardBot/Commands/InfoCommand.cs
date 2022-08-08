namespace HowardBot.Commands
{
	class InfoCommand : Command
	{
		public InfoCommand() { }

		private readonly string info = $"I am Howard, everyone's favorite goat from Twilight Princess. I allow for some fun stuff, such as custom channel point rewards, like screenflip! Use !help to see what I can do! View my code here: https://github.com/Chris-Is-Awesome/HowardBot";

		public override string Run(string[] args)
		{
			return info;
		}
	}
}
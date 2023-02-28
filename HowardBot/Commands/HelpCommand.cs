namespace HowardBot.Commands
{
	class HelpCommand : Command
	{
		private readonly string info = $"There are some custom channel point rewards that trigger visual effects, such as screenflip, pixelate, and more. Try them out by just redeeming the reward! Run !commands to get a list of commands I can do.";

		public override string Run(string[] args)
		{
			return info;
		}
	}
}
namespace HowardBot.Commands
{
	class DisruptCommand : Command
	{
		private readonly string info = "I'm playing with Disrupt! This is similar to Crowd Control in that you can interact with my game, but it's done via channel point rewards. Redeem a reward to see what happens!";

		public override string Run(string[] args)
		{
			return info;
		}
	}
}
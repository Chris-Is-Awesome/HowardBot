namespace HowardBot.Commands
{
	class WhoopCommand : Command
	{
		public WhoopCommand()
		{
			failureMessages = Utility.DeserializeJSON<string[]>(@".\Data\WhoopData.json");
		}

		private const int goatInChance = 15;
		private readonly string[] failureMessages;

		public override string Run(string[] args)
		{
			int randNum = Utility.GetRandomNumberInRange(1, 100);

			// Goat in
			if (randNum <= goatInChance)
				return "/me GOAT IN!";

			// If Howard
			randNum = Utility.GetRandomNumberInRange(0, failureMessages.Length - 1);
			return $"/me {failureMessages[randNum]}";
		}
	}
}
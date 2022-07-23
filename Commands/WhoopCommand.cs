namespace HowardBot.Commands
{
	class WhoopCommand : ICommand
	{
		public WhoopCommand()
		{
			data = Utility.DeserializeJSON<WhoopData>(@".\Data\WhoopData.json");
		}

		private struct WhoopData
		{
			public string[] FailureMessages { get; set; }
		}

		private const int goatInChance = 15;
		private WhoopData data;

		public string Run(string[] args)
		{
			int randNum = Utility.GetRandomNumberInRange(0, 100);

			// Goat in
			if (randNum <= goatInChance)
				return "/me GOAT IN!";

			// If Howard
			randNum = Utility.GetRandomNumberInRange(0, data.FailureMessages.Length - 1);
			return $"/me {data.FailureMessages[randNum]}";
		}
	}
}
namespace HowardBot.Commands
{
	class WhoopCommand : ICommand
	{
		private const int goatInChance = 15;
		private readonly string[] failureResults = new string[]
		{
			"Backwards long jumps out of existence and crashes your game...",
			"Starts to go in, then backs out at last second...",
			"Stays sideways at entrance to barn so I don't even attempt to enter...",
			"Ignores your whoop...",
			"Randomly gets triggered and attacks you...",
			"Kicks you in the groin...",
			"Eats grass just to mock you...",
			"Sits down and grooms self...",
			"Faces the wrong way...",
			"Attempts to enter barn, but gets pushed out by other goats...",
			"Does a backflip...",
			"Runs in circles for hours before tackling you...",
			"Closes barn doors so no other goats can get in...",
			"Whoops you...",
			"Enters barn, but clips out of bounds and unloads, softlocking you..."
		};

		public string Run(string[] args, string userName)
		{
			int randNum = Utility.GetRandomNumberInRange(0, 100);

			// Goat in
			if (randNum <= goatInChance)
				return $"@{userName}: GOAT IN!";

			// If Howard
			randNum = Utility.GetRandomNumberInRange(0, failureResults.Length - 1);
			return $"/me @{userName}: {failureResults[randNum]}";
		}
	}
}
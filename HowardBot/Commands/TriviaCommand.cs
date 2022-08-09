using System.Collections.Generic;
using System.Linq;

namespace HowardBot.Commands
{
	class TriviaCommand : Command
	{
		public TriviaCommand()
		{
			data = Utility.DeserializeJSON<List<TriviaData>>(@".\HowardBot\Data\TriviaData.json");
		}

		private readonly List<TriviaData> data;

		public override string Run(string[] args)
		{
			List<string> allTrivia = new List<string>();

			// Add all trivia to single pool
			foreach (TriviaData gameData in data)
				allTrivia.AddRange(gameData.trivia);

			// Get random trivia
			int randNum = Utility.GetRandomNumberInRange(0, allTrivia.Count - 1);
			string randTrivia = allTrivia[randNum];
			string game = data.Find(x => x.trivia.Contains(randTrivia)).game;

			return $"/me In {game}, {randTrivia}";
		}

		private struct TriviaData
		{
			public readonly string game;
			public readonly string[] trivia;

			public TriviaData(string game, string[] trivia)
			{
				this.game = game;
				this.trivia = trivia;
			}
		}
	}
}
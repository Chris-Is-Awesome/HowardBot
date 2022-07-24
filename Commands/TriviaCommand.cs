using System.Collections.Generic;
using System.Linq;

namespace HowardBot.Commands
{
	class TriviaCommand : ICommand
	{
		public TriviaCommand()
		{
			data = Utility.DeserializeJSON<List<TriviaData>>(@".\Data\TriviaData.json");
			Debug.Log(data[0].trivia.Length);
		}

		private struct TriviaData
		{
			public string game;
			public string[] trivia;
		}

		private readonly List<TriviaData> data;

		public string Run(string[] args)
		{
			List<string> allTrivia = new List<string>();

			// Add all trivia to single pool
			foreach (TriviaData gameData in data)
			{
				allTrivia.AddRange(gameData.trivia);
			}

			// Get random trivia
			int randNum = Utility.GetRandomNumberInRange(0, allTrivia.Count - 1);
			string randTrivia = allTrivia[randNum];
			string game = data.Find(x => x.trivia.Contains(randTrivia)).game;

			return $"/me In {game}, {randTrivia}";
		}
	}
}
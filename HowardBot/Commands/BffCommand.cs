using System;
using System.Collections.Generic;

namespace HowardBot.Commands
{
	class BffCommand : Command
	{
		public BffCommand()
		{
			data = Utility.DeserializeJSON<Data>(@".\Data\BffData.json");
		}

		private readonly Data data;

		public override string Run(string[] args)
		{
			string randGame = GetRandomGame();
			string randFriend = GetRandomFriend(randGame);

			// If Howard
			if (randGame.EndsWith("Twilight Princess") && randFriend == "Howard")
			{
				int randNum = Utility.GetRandomNumberInRange(1, 1000);

				if (randNum == 1)
					return "/me You try to befriend... me?! W-Why, I'm so honored! Fine... I guess you win. <20s goats for you!";

				return "/me You try to befriend... me?! W-Why, I'm so honored! But no, fuck you. *tackles*";
			}

			bool doFriend = Convert.ToBoolean(Utility.GetRandomNumberInRange(0, 1));
			string outcome = $"/me You try to befriend {randFriend} ({randGame})... ";

			// If accept
			if (doFriend)
			{
				string[] outcomes = data.outcomes.accept;
				int randOutcomeId = Utility.GetRandomNumberInRange(0, outcomes.Length - 1);
				string randOutcome = outcomes[randOutcomeId];
				outcome += "They accept! " + randOutcome;
			}
			// If reject
			else
			{
				string[] outcomes = data.outcomes.reject;
				int randOutcomeId = Utility.GetRandomNumberInRange(0, outcomes.Length - 1);
				string randOutcome = outcomes[randOutcomeId];
				outcome += "They reject! " + randOutcome;
			}

			// Handle interpolation
			outcome = outcome.Replace("{name}", randFriend);

			if (outcome.Contains("{randName}"))
			{
				string otherRandFriend = GetRandomFriend();
				string otherRandFriendGame = data.games.Find(x => Array.Exists(x.friends, y => y == otherRandFriend)).game;

				outcome = outcome.Replace("{randName}", $"{otherRandFriend} ({otherRandFriendGame})");
			}

			return outcome;
		}

		private string GetRandomGame()
		{
			int randGameId = Utility.GetRandomNumberInRange(0, data.games.Count - 1);
			return data.games[randGameId].game;
		}

		private string GetRandomFriend(string game = "")
		{
			// Get a friend from any game
			if (string.IsNullOrEmpty(game))
				return GetRandomFriend(GetRandomGame());

			// Get a friend from specified game
			string[] friends = data.games.Find(x => x.game == game).friends;
			int randFriendId = Utility.GetRandomNumberInRange(0, friends.Length - 1);

			return friends[randFriendId];
		}

		private struct Data
		{
			public struct GameData
			{
				public readonly string game;
				public readonly string[] friends;

				public GameData(string game, string[] friends)
				{
					this.game = game;
					this.friends = friends;
				}
			}

			public struct OutcomeData
			{
				public readonly string[] accept;
				public readonly string[] reject;

				public OutcomeData(string[] accept, string[] reject)
				{
					this.accept = accept;
					this.reject = reject;
				}
			}

			public readonly List<GameData> games;
			public readonly OutcomeData outcomes;

			public Data(List<GameData> games, OutcomeData outcomes)
			{
				this.games = games;
				this.outcomes = outcomes;
			}
		}
	}
}
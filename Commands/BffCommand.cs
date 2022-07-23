using System;
using System.Collections.Generic;

namespace HowardBot.Commands
{
	class BffCommand : ICommand
	{
		public BffCommand()
		{
			data = Utility.DeserializeJSON<BffData>(@".\Data\BffData.json");
		}

		private struct BffData
		{
			public struct GameData
			{
				public string Game { get; set; }
				public List<string> Friends { get; set; }
			}

			public struct OutcomeData
			{
				public List<string> Accept { get; set; }
				public List<string> Reject { get; set; }
			}

			public List<GameData> Games { get; set; }
			public OutcomeData Outcomes { get; set; }
		}

		private readonly BffData data;

		public string Run(string[] args)
		{
			string randGame = GetRandomGame();
			string randFriend = GetRandomFriend(randGame);

			// If Howard
			if (randGame.EndsWith("Twilight Princess") && randFriend == "Howard")
				return "/me You try to befriend... me?! W-Why, I'm so honored! But no, fuck you. *tackles*";

			bool doFriend = Convert.ToBoolean(Utility.GetRandomNumberInRange(0, 1));
			string outcome = $"/me You try to befriend {randFriend} ({randGame})... ";

			// If accept
			if (doFriend)
			{
				List<string> outcomes = data.Outcomes.Accept;
				int randOutcomeId = Utility.GetRandomNumberInRange(0, outcomes.Count - 1);
				string randOutcome = outcomes[randOutcomeId];
				outcome += "They accept! " + randOutcome;
			}
			// If reject
			else
			{
				List<string> outcomes = data.Outcomes.Reject;
				int randOutcomeId = Utility.GetRandomNumberInRange(0, outcomes.Count - 1);
				string randOutcome = outcomes[randOutcomeId];
				outcome += "They reject! " + randOutcome;
			}

			// Handle interpolation
			outcome = outcome.Replace("{name}", randFriend);

			if (outcome.Contains("{randName}"))
			{
				string otherRandFriend = GetRandomFriend();
				string otherRandFriendGame = data.Games.Find(x => x.Friends.Contains(otherRandFriend)).Game;

				outcome = outcome.Replace("{randName}", $"{otherRandFriend} ({otherRandFriendGame})");
			}

			return outcome;
		}

		private string GetRandomGame()
		{
			int randGameId = Utility.GetRandomNumberInRange(0, data.Games.Count - 1);
			return data.Games[randGameId].Game;
		}

		private string GetRandomFriend(string game = "")
		{
			// Get a friend from any game
			if (string.IsNullOrEmpty(game))
				return GetRandomFriend(GetRandomGame());

			// Get a friend from specified game
			List<string> friends = data.Games.Find(x => x.Game == game).Friends;
			int randFriendId = Utility.GetRandomNumberInRange(0, friends.Count - 1);
			return friends[randFriendId];
		}

		private void DebugStats()
		{
			Debug.Log($"[Stats for: {nameof(BffCommand)}]", false);
		}
	}
}
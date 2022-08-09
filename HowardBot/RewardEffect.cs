using HowardBot.TwitchSucks;
using System;

namespace HowardBot
{
	class RewardEffect
	{
		public delegate void Func(RewardEffect effect);

		public Func onEffectStop;

		public string Name { get; }
		public string RewardId { get; }
		public Reward Reward { get; private set; }

		/// <summary>
		/// Creates & initializes a new RewardEffect
		/// </summary>
		/// <param name="name">User-friendly name, just used for outputting/debugging</param>
		/// <param name="rewardId">The ID of the reward from Twitch (string)</param>
		public RewardEffect(string name, string rewardId)
		{
			Name = name;
			RewardId = rewardId;

			GetReward();
		}

		private async void GetReward()
		{
			var response = await API.Instance.GetChannelPointRewards(Bot.ChannelId);

			if (response != null)
			{
				var reward = Array.Find(response, x => x.Id == RewardId);
				
				if (reward != null)
					Reward = new Reward(Array.Find(response, x => x.Id == RewardId));
			}
		}
	}
}
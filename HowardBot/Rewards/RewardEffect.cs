namespace HowardBot.Rewards
{
	public class RewardEffect
	{
		public delegate void Func(RewardEffect effect);
		public Func onEffectStop;

		public Reward Reward { get; }

		public RewardEffect(RewardHandler.RewardData.Reward rewardData)
		{
			Reward = new Reward()
			{
				OnTwitch = !rewardData.noTwitchReward,
				Title = rewardData.title,
				Description = rewardData.description,
				Cost = rewardData.cost,
				IsEnabled = rewardData.isEnabled,
				BackgroundColor = rewardData.backgroundColor,
				IsTextRequired = rewardData.isTextRequired,
				GlobalCooldownSeconds = rewardData.globalCooldownSeconds,
				MaxRedemptionsPerStream = rewardData.maxRedemptionsPerStream,
				MaxRedemptionsPerUserPerStream = rewardData.maxRedemptionsPerUserPerStream,
				SkipRequestQueue = rewardData.skipRequestQueue
			};

			if (!rewardData.noTwitchReward)
				Reward.AddRewardToTwitch();
		}
	}
}
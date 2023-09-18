namespace HowardBot.Rewards
{
	public class RewardEffect_OLD
	{
		public delegate void Func(RewardEffect_OLD effect);
		public Func onEffectStop;

		public Reward_OLD Reward { get; }

		public RewardEffect_OLD(RewardHandler_OLD.RewardData.Reward rewardData)
		{
			Reward = new Reward_OLD()
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
using TwitchLib.Api.Helix.Models.ChannelPoints;

namespace HowardBot.TwitchSucks
{
	class Reward : CustomReward
	{
		public new bool IsEnabled
		{
			get
			{
				return base.IsEnabled;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public Reward(CustomReward reward)
		{
			ShouldRedemptionsSkipQueue = reward.ShouldRedemptionsSkipQueue;
			IsInStock = reward.IsInStock;
			IsPaused = reward.IsPaused;
			GlobalCooldownSetting = reward.GlobalCooldownSetting;
			MaxPerUserPerStreamSetting = reward.MaxPerUserPerStreamSetting;
			MaxPerStreamSetting = reward.MaxPerStreamSetting;
			IsUserInputRequired = reward.IsUserInputRequired;
			IsEnabled = reward.IsEnabled;
			RedemptionsRedeemedCurrentStream = reward.RedemptionsRedeemedCurrentStream;
			BackgroundColor = reward.BackgroundColor;
			Image = reward.Image;
			Cost = reward.Cost;
			Prompt = reward.Prompt;
			Title = reward.Title;
			Id = reward.Id;
			BroadcasterName = reward.BroadcasterName;
			BroadcasterLogin = reward.BroadcasterLogin;
			BroadcasterId = reward.BroadcasterId;
			DefaultImage = reward.DefaultImage;
			CooldownExpiresAt = reward.CooldownExpiresAt;
		}
	}
}
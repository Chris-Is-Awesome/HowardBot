using System;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace HowardPlays
{
	class RewardHandler
	{
		TwitchPubSub client;

		public RewardHandler(TwitchPubSub pubsubClient)
		{
			client = pubsubClient;
			client.ListenToChannelPoints(Bot.channelId);
			client.OnChannelPointsRewardRedeemed += OnRewardRedeemed;
		}

		private void OnRewardRedeemed(object sender, OnChannelPointsRewardRedeemedArgs e)
		{
			var redemption = e.RewardRedeemed.Redemption;
			var reward = redemption.Reward;
			var userWhoRedeemed = redemption.User;
			var userInput = redemption.UserInput;

			Console.WriteLine($"{Bot.GetTimestamp()} {userWhoRedeemed.DisplayName} redeemed {reward.Title}!");
		}
	}
}
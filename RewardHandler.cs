using AutoHotkey.Interop;
using System;
using System.Threading.Tasks;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace HowardPlays
{
	class RewardHandler
	{
		AutoHotkeyEngine ahk;
		TwitchPubSub client;

		public RewardHandler(TwitchPubSub pubsubClient, AutoHotkeyEngine ahk)
		{
			this.ahk = ahk;
			client = pubsubClient;
			client.ListenToChannelPoints(Bot.channelId);
			client.OnChannelPointsRewardRedeemed += OnRewardRedeemed;
		}

		private async void OnRewardRedeemed(object sender, OnChannelPointsRewardRedeemedArgs args)
		{
			var redemption = args.RewardRedeemed.Redemption;
			var reward = redemption.Reward;
			var userWhoRedeemed = redemption.User;
			var userInput = redemption.UserInput;

			Bot.SendMessage($"{userWhoRedeemed.DisplayName} redeemed {reward.Title}");
			Console.WriteLine($"{Bot.GetTimestamp()} [Redemption] {userWhoRedeemed.DisplayName} redeemed {reward.Title}");

			// Parse rewards for possible custom events
			switch (reward.Title)
			{
				case "Screenflip": // Flips my screen upside down for 30 seconds
					ahk.ExecRaw("Send {Alt down}{shift down}{Down down}"); // Flip screen
					ahk.ExecRaw("Send {Alt up}{shift up}{Down up}"); // Flip screen
					await Task.Delay(TimeSpan.FromSeconds(30)); // Wait
					ahk.ExecRaw("Send {Alt down}{shift down}{Up down}"); // Unflip screen
					ahk.ExecRaw("Send {Alt up}{shift up}{Up up}"); // Unflip screen

					break;
			}
		}
	}
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HowardBot.Rewards;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using TwitchLib.PubSub.Models.Responses.Messages;
using TwitchLib.PubSub.Models.Responses.Messages.Redemption;
using Reward = TwitchLib.PubSub.Models.Responses.Messages.Redemption.Reward;

namespace HowardBot
{
	public class RewardHandler_OLD : Singleton<RewardHandler_OLD>
	{
		private const string rewardDataPath = @".\HowardBot\Data\RewardData_OLD.json";
		private readonly TwitchPubSub client;
		private readonly List<RewardEffect_OLD> effects = new();

		private Timer timer;
		private List<RewardEffect_OLD> activeEffects = new();
		private List<RewardEffect_OLD> effectsInQueue = new();
		private int redemptionsThisStream;
		private int pointsSpentThisStream;

		public RewardHandler_OLD()
		{
			client = TwitchHandler.Instance.PubSubClient;
			client.ListenToChannelPoints(Bot.ChannelId);
			client.OnChannelPointsRewardRedeemed += OnRewardRedeemed;
		}

		public async Task CreateCustomRewards()
		{
			RewardData rewardData = Utility.DeserializeJSON<RewardData>(rewardDataPath);
			bool enableAll = await EnableAllRewards();

			foreach (RewardData.Reward reward in rewardData.rewards)
			{
				// Add audio effects
				if (reward.audioData != null)
					effects.Add(new AudioEffect_OLD(reward, reward.audioData));
				// Add visual effects
				else if (reward.visualData != null && enableAll)
					effects.Add(new VisualEffect_OLD(reward, reward.visualData));
			}
		}

		public async Task DeleteCustomRewards()
		{
			foreach (RewardEffect_OLD effect in effects)
			{
				//if (effect.Reward.OnTwitch)
				//await API.Instance.DeleteCustomReward(Bot.ChannelId, effect.Reward);
			}

			await Utility.WaitForMilliseconds(0);

			effects.Clear();
		}

		private async Task<bool> EnableAllRewards()
		{
			var response = await API.Instance.GetChannelInfo(Bot.ChannelId);

			if (response != null)
			{
				string title = response.Title.ToLower();
				bool titleCheck = title.Contains("dev") || title.Contains("science") || title.Contains("speedrun") || title.Contains("Race");
				bool tagsCheck = response.Tags.Any(x => x == "Speedrun" || x == "Programming");
				bool gameCheck = response.GameName == "Science & Technology";

				if (tagsCheck || titleCheck || gameCheck)
					return false;

				return true;
			}

			return false;
		}

		// When a user redeems a channel point reward
		private void OnRewardRedeemed(object sender, OnChannelPointsRewardRedeemedArgs args)
		{
			if (Bot.AmILive)
			{
				Redemption redemption = args.RewardRedeemed.Redemption;
				Reward reward = redemption.Reward;
				User userWhoRedeemed = redemption.User;
				string userInput = redemption.UserInput;
				pointsSpentThisStream += reward.Cost;

				LogRedemption(reward, userWhoRedeemed);

				// Start timer to handle effect queue
				if (timer == null)
				{
					timer = new Timer((e) =>
					{
						CheckEffectsInQueue();
					}, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
				}

				// Parse rewards for custom event

				// If reward is random visual effect
				if (reward.Title.EndsWith("Random Visual Effect"))
				{
					List<RewardEffect_OLD> visualEffects = effects.FindAll(x => x.GetType() == typeof(VisualEffect_OLD) && ((VisualEffect_OLD)x).HotkeyNum >= 0);
					int randNum = Utility.GetRandomNumberInRange(0, visualEffects.Count - 1);
					TryEffect(visualEffects[randNum], reward, userWhoRedeemed, true, userInput);
				}
				else if (reward.Title.StartsWith('['))
				{
					RewardEffect_OLD effect = effects.Find(x => x.Reward.Title == reward.Title);

					if (effect != null)
						TryEffect(effect, reward, userWhoRedeemed, false, userInput);
				}
			}
		}

		private void TryEffect(RewardEffect_OLD effect, Reward reward, User userWhoRedeemed, bool random, string userInput)
		{
			string output = $"{userWhoRedeemed.DisplayName} redeemed {reward.Title}";
			if (random) output += $" - {effect.Reward.Title}";
			output += "!";

			// Start effect if effect isn't already active or effect type isn't active
			if (!activeEffects.Contains(effect) && !activeEffects.Any(x => x.GetType() == effect.GetType()))
			{
				TwitchHandler.SendMessage(output);
				Start(effect, userInput);
			}
			// Add effect to queue
			else
			{
				effectsInQueue.Add(effect);
				TwitchHandler.SendMessage(output += $" Only one effect of each type can be active at once, so {effect.Reward.Title} will be added to the end of the queue at position {effectsInQueue.Count}");
			}
		}

		public void Start(RewardEffect_OLD effect, string userInput = "")
		{
			bool invalid = false;

			switch (effect)
			{
				case VisualEffect_OLD visualEffect:

					visualEffect.StartFunc.Invoke();
					break;

				case AudioEffect_OLD audioEffect:

					if (audioEffect.random)
						audioEffect.StartRandomSoundFunc.Invoke(audioEffect.type);
					else
						audioEffect.StartSoundFunc.Invoke(audioEffect.type, audioEffect.name);

					break;

				default:

					Debug.LogError($"Tried to start reward effect '{effect.Reward.Title}' with an unknown effect type.");
					invalid = true;
					break;
			}

			if (!invalid)
			{
				activeEffects.Add(effect);
				effect.onEffectStop += OnEffectStop;
			}
		}

		private void CheckEffectsInQueue()
		{
			for (int i = 0; i < effectsInQueue.Count; i++)
			{
				RewardEffect_OLD effect = effectsInQueue[i];

				// Don't start effect of same type currently active
				if (activeEffects.Contains(effect) || activeEffects.Any(x => x.GetType() == effect.GetType()))
					continue;

				effectsInQueue.RemoveAt(i);

				// Handle output
				string output = $"Started effect {effect.Reward.Title} from queue.";
				if (effectsInQueue.Count > 0)
				{
					if (effectsInQueue.Count < 2) output += $" {effectsInQueue.Count} effect remains in queue.";
					else output += $" {effectsInQueue.Count} effects remain in queue.";
				}
				else output += " Queue is empty.";

				TwitchHandler.SendMessage(output);
				Start(effect);
				break;
			}
		}

		private void OnEffectStop(RewardEffect_OLD effect)
		{
			activeEffects.Remove(effect);
			effect.onEffectStop -= OnEffectStop;
		}

		private void LogRedemption(Reward reward, User user)
		{
			// Update stats
			redemptionsThisStream++;

			// Log text
			Bot.Instance.ReplaceLineInFile("Channel point redemptions", $"Channel point redemptions: {redemptionsThisStream}");
			Bot.Instance.ReplaceLineInFile("Channel points spent", $"Channel points spent: {pointsSpentThisStream}");
			Bot.Instance.AppendToLogFile($"[Redemption] {user.DisplayName} redeemed {reward.Title}");

			Debug.Log($"[Redemption] {user.DisplayName} redeemed {reward.Title}");
		}

		public readonly struct RewardData
		{
			public readonly Reward[] rewards;

			public RewardData(Reward[] rewards)
			{
				this.rewards = rewards;
			}

			public readonly struct Reward
			{
				public readonly bool noTwitchReward;
				public readonly string title;
				public readonly string description;
				public readonly int cost;
				public readonly bool isEnabled;
				public readonly string backgroundColor;
				public readonly bool isTextRequired;
				public readonly int globalCooldownSeconds;
				public readonly int maxRedemptionsPerStream;
				public readonly int maxRedemptionsPerUserPerStream;
				public readonly bool skipRequestQueue;
				public readonly AudioEffect_OLD.EffectData audioData;
				public readonly VisualEffect_OLD.EffectData visualData;

				public Reward(bool noTwitchReward, string title, string description, int cost, bool isEnabled, string backgroundColor, bool isTextRequired, int globalCooldownSeconds, int maxRedemptionsPerStream, int maxRedemptionsPerUserPerStream, bool skipRequestQueue, AudioEffect_OLD.EffectData audioData, VisualEffect_OLD.EffectData visualData)
				{
					this.noTwitchReward = noTwitchReward;
					this.title = title;
					this.description = description;
					this.cost = cost;
					this.isEnabled = isEnabled;
					this.backgroundColor = backgroundColor;
					this.isTextRequired = isTextRequired;
					this.globalCooldownSeconds = globalCooldownSeconds;
					this.maxRedemptionsPerStream = maxRedemptionsPerStream;
					this.maxRedemptionsPerUserPerStream = maxRedemptionsPerUserPerStream;
					this.skipRequestQueue = skipRequestQueue;
					this.audioData = audioData;
					this.visualData = visualData;
				}
			}
		}
	}
}
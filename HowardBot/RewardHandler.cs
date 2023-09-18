﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HowardBot.Rewards;
using Newtonsoft.Json.Linq;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Models.Responses.Messages.Redemption;

namespace HowardBot
{
	class RewardHandler : Singleton<RewardHandler>
	{
		private const string rewardDataPath = @".\HowardBot\Data\RewardData.json";
		private const int queueTimerInterval = 1000; // Milliseconds
		private readonly TwitchPubSub pubsub;
		private readonly List<CustomReward> rewards = new();
		private readonly List<CustomReward> rewardsActive = new();
		private readonly List<QueuedReward> rewardsInQueue = new();
		private Timer queueTimer;

		// Stats
		private int redemptionsThisStream;
		private int pointsSpentThisStream;

		public RewardHandler()
		{
			pubsub = TwitchHandler.Instance.PubSubClient;
			pubsub.OnChannelPointsRewardRedeemed += OnRewardRedeemed;

			InitRewards();
		}

		/// <summary>
		/// Creates all the custom channel point rewards and adds them to Twitch
		/// </summary>
		public async Task CreateCustomRewards()
		{
			// Get all channel point rewards currently on Twitch
			var twitchRewards = await API.Instance.GetChannelPointRewards(Bot.ChannelId);

			foreach (CustomReward reward in rewards)
			{
				// Gets the reward object from Twitch
				var twitchReward = twitchRewards.FirstOrDefault(x => x.Id == reward.Id);

				// If reward is already on Twitch, enable it
				if (twitchReward != null)
				{
					if (reward.DoEnable)
						await reward.Enable(twitchReward);
				}
				// If reward is not on Twitch, add it
				else
					await reward.AddToTwitch();
			}
		}

		/// <summary>
		/// Removes all the custom channel point rewards from Twitch
		/// </summary>
		public async Task DisableCustomRewards()
		{
			foreach (CustomReward reward in rewards)
				await reward.Disable();
		}

		/// <summary>
		/// Initializes the rewards (parses JSON)
		/// </summary>
		private void InitRewards()
		{
			// Parse JSON to object
			CustomReward.RewardJSONObject rewardJSONObject = Utility.DeserializeJSON<CustomReward.RewardJSONObject>(rewardDataPath);

			// For each reward found in JSON
			foreach (CustomReward reward in rewardJSONObject.Rewards)
			{
				rewards.Add(reward);

				// Determine the effect object
				switch (reward.RewardType)
				{
					case CustomReward.Type.Audio:
						reward.Effect = ((JObject)reward.Data).ToObject<AudioReward>();
						break;
					case CustomReward.Type.OBS:
						reward.Effect = ((JObject)reward.Data).ToObject<OBSReward>();
						break;
				}
			}
		}

		/// <summary>
		/// Runs when a channel point reward is redeemed
		/// </summary>
		private void OnRewardRedeemed(object sender, TwitchLib.PubSub.Events.OnChannelPointsRewardRedeemedArgs e)
		{
			if (Bot.AmILive)
			{
				var redemption = e.RewardRedeemed.Redemption;
				var reward = redemption.Reward;
				CustomReward customReward = rewards.FirstOrDefault(x => x.Id == reward.Id);

				// If is a custom reward that has an effect
				if (customReward != null && customReward.Effect != null)
				{
					// If effect can start, start it
					if (CanStartEffect(customReward))
						StartEffect(customReward, redemption, false);
					// If effect can't start (usually due to queue)
					else
					{
						rewardsInQueue.Add(new QueuedReward(customReward, redemption));
						TwitchHandler.SendMessage($"{redemption.User.DisplayName} redeemed {reward.Title}, but it can't start now due to the other effects active, so it's been added to the queue at position {rewardsInQueue.Count}!");

						// Start timer to handle reward queue
						queueTimer ??= new Timer((e) =>
							{
								CheckEffectsInQueue();
							}, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(queueTimerInterval));
					}
				}
				else
					LogRedemption(redemption, false);
			}
		}

		/// <summary>
		/// Checks if an effect in the queue can start, then starts it if it can
		/// </summary>
		private void CheckEffectsInQueue()
		{
			if (rewardsInQueue.Count > 0)
			{
				QueuedReward queuedReward = rewardsInQueue[0];

				if (CanStartEffect(queuedReward.reward, true))
				{
					rewardsInQueue.RemoveAt(0);
					StartEffect(queuedReward.reward, queuedReward.redemption, true);

					// Stop queue timer
					if (rewardsInQueue.Count == 0 && queueTimer != null)
					{
						queueTimer.Dispose();
						queueTimer = null;
					}
				}
			}
		}

		/// <summary>
		/// Starts the reward's effect
		/// </summary>
		private void StartEffect(CustomReward reward, Redemption redemption, bool startedFromQueue)
		{
			reward.Effect.TriggerEffect(redemption.UserInput);
			reward.Effect.OnEffectFinished += OnRewardEffectFinished;
			rewardsActive.Add(reward);
			LogRedemption(redemption, startedFromQueue);
		}

		private void OnRewardEffectFinished(CustomRewardEffect effect)
		{
			CustomReward rewardFinished = rewardsActive.Find(x => x.Effect == effect);
			rewardsActive.Remove(rewardFinished);
			rewardFinished.Effect.OnEffectFinished -= OnRewardEffectFinished;
		}

		/// <summary>
		/// Logs the redemption to chat and to log file
		/// </summary>
		private void LogRedemption(Redemption redemption, bool startedFromQueue)
		{
			Reward reward = redemption.Reward;
			string redemptionOutput;
			redemptionsThisStream++;
			pointsSpentThisStream += reward.Cost;

			// Modify output based on amount in queue
			if (!startedFromQueue)
				redemptionOutput = $"{redemption.User.DisplayName} redeemed {reward.Title}!";
			else
			{
				redemptionOutput = $"{reward.Title} redeemed by {redemption.User.DisplayName} has been started from queue. ";

				// Pluralized output
				if (rewardsInQueue.Count > 0)
					redemptionOutput += string.Format("There {0} {1} reward{2} left in the queue.", rewardsInQueue.Count > 1 ? "are" : "is", rewardsInQueue.Count, rewardsInQueue.Count > 1 ? "s" : "");
				// Singuluar output
				else
					redemptionOutput += "The queue is empty.";
			}

			TwitchHandler.SendMessage(redemptionOutput);

			// Log to log file
			Bot.Instance.ReplaceLineInFile("Channel point redemptions", $"Channel point redemptions: {redemptionsThisStream}");
			Bot.Instance.ReplaceLineInFile("Channel points spent", $"Channel points spent: {pointsSpentThisStream}");
			Bot.Instance.AppendToLogFile($"[Redemption] {redemption.User.DisplayName} redeemed {reward.Title}");
		}

		/// <summary>
		/// Can the reward's effect start?
		/// </summary>
		/// <returns>True if it can start; false otherwise</returns>
		private bool CanStartEffect(CustomReward reward, bool fromQueue = false)
		{
			/* Can NOT start effect if:
			// - The reward is already active, OR
			// - The reward is an OBS one, OR
			// - The reward is an audio one AND there's an audio already playing or in the queue
			*/

			if (rewardsActive.Contains(reward))
				return false;

			if (reward.RewardType == CustomReward.Type.OBS)
				return true;

			if (reward.RewardType == CustomReward.Type.Audio)
			{
				bool isAudioActive = rewardsActive.Any(x => x.RewardType == CustomReward.Type.Audio);

				if (isAudioActive)
					return false;
				else
					return fromQueue || !rewardsInQueue.Any(x => x.reward.RewardType == CustomReward.Type.Audio);
			}

			return false;
		}

		private readonly struct QueuedReward
		{
			public readonly CustomReward reward;
			public readonly Redemption redemption;

			public QueuedReward(CustomReward reward, Redemption redemption)
			{
				this.reward = reward;
				this.redemption = redemption;
			}
		}
	}
}
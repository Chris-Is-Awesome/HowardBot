using AutoHotkey.Interop;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using TwitchLib.PubSub.Models.Responses.Messages;
using TwitchLib.PubSub.Models.Responses.Messages.Redemption;

namespace HowardPlays
{
	class RewardHandler
	{
		public RewardHandler(TwitchPubSub pubsubClient, AutoHotkeyEngine ahk)
		{
			RewardHandler.ahk = ahk;
			client = pubsubClient;
			client.ListenToChannelPoints(Bot.channelId);
			client.OnChannelPointsRewardRedeemed += OnRewardRedeemed;
		}

		private class VisualEffect
		{
			public string name;
			public int id;
			public float duration;
			public bool isActive;

			public VisualEffect(string name, int id, float duration)
			{
				this.name = name;
				this.id = id;
				this.duration = duration;
			}

			public async Task Start(bool startedFromQueue)
			{
				activeEffect = this;

				await Utility.WaitForSeconds(1);

				ahk.ExecRaw($"Send {{Ctrl down}} {{0 down}} {{{id} down}} {{Left}} ");
				await Utility.WaitForMilliseconds(10);
				ahk.ExecRaw($"Send {{Ctrl up}} {{0 up}} {{{id} up}}");

				if (startedFromQueue)
					Bot.SendMessage($"Started effect {name} from queue. {effectsInQueue.Count} left in queue.");

				await Utility.WaitForSeconds(duration);
				await Stop();
			}

			public async Task Stop()
			{
				ahk.ExecRaw($"Send {{Ctrl down}} {{0 down}} {{{id} down}} {{Right}}");
				await Utility.WaitForMilliseconds(10);
				ahk.ExecRaw($"Send {{Ctrl up}} {{0 up}} {{{id} up}}");

				activeEffect = null;
			}
		}

		private static AutoHotkeyEngine ahk;
		private readonly TwitchPubSub client;
		private static VisualEffect activeEffect;
		private static List<VisualEffect> effectsInQueue = new List<VisualEffect>();
		private List<VisualEffect> visualEffects = new List<VisualEffect>()
		{
			{ new VisualEffect("qilʇnɘɘɿɔƧ", 1, 60) },
			{ new VisualEffect("【﻿Ｗ　Ｉ　Ｄ　Ｅ】Mode", 2, 60) },
			{ new VisualEffect("Pinhole", 3, 60) },
			{ new VisualEffect("Pixelate", 4, 60) },
			{ new VisualEffect("Inversion", 5, 60) },
			{ new VisualEffect("Black & White", 6, 60) },
			{ new VisualEffect("Cell-Shaded", 7, 60) },
			{ new VisualEffect("VHS", 8, 60) }
		};
		private Timer timer;

		private async void OnRewardRedeemed(object sender, OnChannelPointsRewardRedeemedArgs args)
		{
			Redemption redemption = args.RewardRedeemed.Redemption;
			Reward reward = redemption.Reward;
			User userWhoRedeemed = redemption.User;
			string userInput = redemption.UserInput;

			Debug.Log($"[Redemption] {userWhoRedeemed.DisplayName} redeemed {reward.Title}");

			// Start timer to handle effect queue
			if (timer == null)
			{
				timer = new Timer((e) =>
				{
					CheckEffectsInQueue();
				}, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
			}

			// Parse rewards for custom events
			if (reward.Title == "Random Visual Effect")
			{
				int randNum = Utility.GetRandomNumberInRange(0, visualEffects.Count - 1);
				VisualEffect effect = visualEffects[randNum];
				await TryVisualEffect(effect, reward, userWhoRedeemed, true);
			}
			else
			{
				VisualEffect effect = visualEffects.Find(x => x.name == reward.Title);

				if (effect != null)
					await TryVisualEffect(effect, reward, userWhoRedeemed, false);
			}
		}

		private async Task TryVisualEffect(VisualEffect effect, Reward reward, User userWhoRedeemed, bool random)
		{
			string output = $"{userWhoRedeemed.DisplayName} redeemed {reward.Title}";
			if (random) output += $" - {effect.name}";
			output += "!";

			if (activeEffect == null && effectsInQueue.Count < 1)
			{
				// Start effect
				Bot.SendMessage(output);
				await effect.Start(false);
			}
			else
			{
				// Add effect to queue and wait to start it when it can be
				effectsInQueue.Add(effect);
				Bot.SendMessage(output += $" Only one effect can be active at a time, so this will be added to the end of the queue: ({effectsInQueue.Count})");
			}
		}

		// Runs once per second from the time a redemption is first redeemed
		private async void CheckEffectsInQueue()
		{
			if (effectsInQueue.Count > 0 && activeEffect == null)
			{
				VisualEffect effect = effectsInQueue[0];
				effectsInQueue.RemoveAt(0);
				await effect.Start(true);
			}
		}
	}
}
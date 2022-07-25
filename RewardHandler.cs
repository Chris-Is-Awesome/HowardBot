using AutoHotkey.Interop;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using TwitchLib.PubSub.Models.Responses.Messages;
using TwitchLib.PubSub.Models.Responses.Messages.Redemption;

namespace HowardBot
{
	class RewardHandler
	{
		public RewardHandler()
		{
			visualEffects = new List<VisualEffect>()
			{
				{ new VisualEffect("qilʇnɘɘɿɔƧ", 1, 10) },
				{ new VisualEffect("【﻿Ｗ　Ｉ　Ｄ　Ｅ】Mode", 2, 10) },
				{ new VisualEffect("Pinhole", 3, 10) },
				{ new VisualEffect("Pixelate", 4, 10) },
				{ new VisualEffect("Inversion", 5, 10) },
				{ new VisualEffect("Black & White", 6, 10) },
				{ new VisualEffect("Cell-Shaded", 7, 10) },
				{ new VisualEffect("VHS", 8, 10) }
			};

			ahk = Bot.AHK;
			client = Bot.PubSubClient;
			client.ListenToChannelPoints(Bot.ChannelId);
			client.OnChannelPointsRewardRedeemed += OnRewardRedeemed;
		}

		private static AutoHotkeyEngine ahk;
		private static List<VisualEffect> effectsInQueue = new List<VisualEffect>();
		private static VisualEffect activeEffect;

		private readonly TwitchPubSub client;
		private readonly List<VisualEffect> visualEffects;

		private Timer timer;

		// When a user redeems a channel point reward
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

		/// <summary>
		/// Tries to start a visual effect. If the effect can't be started right away, it gets added to the queue.
		/// </summary>
		/// <param name="effect">The visual effect to start</param>
		/// <param name="reward">The Reward object</param>
		/// <param name="userWhoRedeemed">The User object for who redeemed it</param>
		/// <param name="random">Was the effect chosen at random? If true, it adds the randomly chosen effect name to the message sent.</param>
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

		/// <summary>
		/// Checks if any effects in the queue can be started and starts it if so. Runs once per second from the time a redemption is first redeemed.
		/// </summary>
		private async void CheckEffectsInQueue()
		{
			if (effectsInQueue.Count > 0 && activeEffect == null)
			{
				VisualEffect effect = effectsInQueue[0];
				effectsInQueue.RemoveAt(0);
				await effect.Start(true);
			}
		}

		private class VisualEffect
		{
			public string name;
			public int id;
			public float duration;

			public VisualEffect(string name, int id, float duration)
			{
				this.name = name;
				this.id = id;
				this.duration = duration;
			}

			/// <summary>
			/// Starts the effect. If <paramref name="startedFromQueue"/> is true, it sends the queue update message.
			/// </summary>
			/// <param name="startedFromQueue">Does this task come from the queue? If so, it sends the queue update message.</param>
			public async Task Start(bool startedFromQueue)
			{
				activeEffect = this;

				await Utility.WaitForSeconds(1);

				ahk.ExecRaw($"Send {{Ctrl down}} {{0 down}} {{{id} down}}");
				await Utility.WaitForMilliseconds(10);
				ahk.ExecRaw($"Send {{Ctrl up}} {{0 up}} {{{id} up}}");

				if (startedFromQueue)
					Bot.SendMessage($"Started effect {name} from queue. {effectsInQueue.Count} left in queue.");

				await Utility.WaitForSeconds(duration);
				await Stop();
			}

			/// <summary>
			/// Stops the effect.
			/// </summary>
			public async Task Stop()
			{
				ahk.ExecRaw($"Send {{Ctrl down}} {{0 down}} {{{id} down}}");
				await Utility.WaitForMilliseconds(10);
				ahk.ExecRaw($"Send {{Ctrl up}} {{0 up}} {{{id} up}}");

				activeEffect = null;
			}
		}
	}
}
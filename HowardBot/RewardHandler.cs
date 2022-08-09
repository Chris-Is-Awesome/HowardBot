using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;
using TwitchLib.PubSub.Models.Responses.Messages;
using TwitchLib.PubSub.Models.Responses.Messages.Redemption;
using Reward = TwitchLib.PubSub.Models.Responses.Messages.Redemption.Reward;

namespace HowardBot
{
	class RewardHandler
	{
		private readonly TwitchPubSub client;
		private readonly List<RewardEffect> effects;

		private Timer timer;
		private List<RewardEffect> activeEffects = new List<RewardEffect>();
		private List<RewardEffect> effectsInQueue = new List<RewardEffect>();
		private int redemptionsThisStream;
		private int pointsSpentThisStream;

		public RewardHandler()
		{
			if (Bot.AmILive)
			{
				effects = new List<RewardEffect>()
				{
					{ new VisualEffect("qilʇnɘɘɿɔƧ", "595256e2-0c44-403f-8788-73a03192d2df", 1) },
					{ new VisualEffect("【 Ｗ Ｉ Ｄ Ｅ】Mode", "25040c6c-431c-47c2-bdc2-476a9796170a", 2) },
					{ new VisualEffect("Pinhole", "", 3) },
					{ new VisualEffect("Pixelate", "f642e489-43da-48c9-b8cc-0dd4a01ffd16", 4) },
					{ new VisualEffect("Inverted", "", 5) },
					{ new VisualEffect("Black & White", "", 6) },
					{ new VisualEffect("Cell-Shaded", "", 7) },
					{ new VisualEffect("VHS", "", 8) },
					{ new AudioEffect("Play a Random Sound", "1c796bda-34f6-44e3-b823-ce887ab969b6") },
				};

				client = Bot.PubSubClient;
				client.ListenToChannelPoints(Bot.ChannelId);
				client.OnChannelPointsRewardRedeemed += OnRewardRedeemed;
			}
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

				// Parse rewards for custom events

				// If reward is random visual effect
				if (reward.Id == "46490842-681c-467c-a25a-1d62fc71db8e")
				{
					List<RewardEffect> visualEffects = effects.FindAll(x => x.GetType() == typeof(VisualEffect));
					int randNum = Utility.GetRandomNumberInRange(0, visualEffects.Count - 1);
					TryEffect(visualEffects[randNum], reward, userWhoRedeemed, true);
				}
				else
				{
					RewardEffect effect = effects.Find(x => x.Name == reward.Title);

					if (effect != null)
					TryEffect(effect, reward, userWhoRedeemed, false);
				}
			}
		}

		private void TryEffect(RewardEffect effect, Reward reward, User userWhoRedeemed, bool random)
		{
			string output = $"{userWhoRedeemed.DisplayName} redeemed {reward.Title}";
			if (random) output += $" - {effect.Name}";
			output += "!";

			// Start effect if effect isn't already active or effect type isn't active
			if (!activeEffects.Contains(effect) && !activeEffects.Any(x => x.GetType() == effect.GetType()))
			{
				Bot.SendMessage(output);
				Start(effect.Reward, effect);
			}
			// Add effect to queue
			else
			{
				effectsInQueue.Add(effect);
				Bot.SendMessage(output += $" Only one effect of each type can be active at once, so {effect.Name} will be added to the end of the queue at position {effectsInQueue.Count}");
			}
		}

		public void Start(TwitchSucks.Reward reward, RewardEffect effect)
		{
			bool invalid = false;

			switch (effect)
			{
				case VisualEffect visualEffect:

					visualEffect.StartFunc.Invoke();

					break;
				case AudioEffect audioEffect:

					AudioEffect.SoundType type = AudioEffect.SoundType.Sound;
					bool random = false;

					switch(reward.Id)
					{
						case "1c796bda-34f6-44e3-b823-ce887ab969b6":
							random = true;
							break;
					}

					audioEffect.StartFunc.Invoke(type, random);

					break;
				case InputEffect inputEffect:

					inputEffect.StartFunc.Invoke(reward.Prompt);

					break;
				default:
					Debug.LogError($"Tried to start reward effect '{effect.Name}' with an unknown effect type.");
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
				RewardEffect effect = effectsInQueue[i];

				// Don't start effect of same type currently active
				if (activeEffects.Contains(effect) || activeEffects.Any(x => x.GetType() == effect.GetType()))
					continue;

				effectsInQueue.RemoveAt(i);

				// Handle output
				string output = $"Started effect {effect.Name} from queue.";
				if (effectsInQueue.Count > 0)
				{
					if (effectsInQueue.Count < 2) output += $" {effectsInQueue.Count} effect remains in queue.";
					else output += $" {effectsInQueue.Count} effects remain in queue.";
				}
				else output += " Queue is empty.";

				Bot.SendMessage(output);
				Start(effect.Reward, effect);
				break;
			}
		}

		private void OnEffectStop(RewardEffect effect)
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
	}
}
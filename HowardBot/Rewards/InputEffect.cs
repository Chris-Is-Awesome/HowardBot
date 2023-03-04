using AutoHotkey.Interop;
using System.Collections.Generic;

namespace HowardBot.Rewards
{
	public class InputEffect : RewardEffect
	{
		public delegate void EffectFunc(string userInput);

		private readonly AutoHotkeyEngine ahk;
		private readonly List<string> btnNames = new()
		{
			""
		};

		public EffectFunc StartFunc { get { return Start; } }

		public InputEffect(RewardHandler.RewardData.Reward rewardData) : base(rewardData)
		{
			ahk = Bot.AHK;
		}

		private async void Start(string userInput)
		{
			ahk.ExecRaw($"Send {{{userInput} down}}");
			await Utility.WaitForMilliseconds(1000);
			ahk.ExecRaw($"Send {{{userInput} up}}");
			Stop();
			return;

			// Parse user input
			if (btnNames.Contains(userInput.ToLower()))
			{
				ahk.ExecRaw($"Send {{userInput}}");
				return;
			}

			Stop();
			return;
		}

		private void Stop()
		{
			onEffectStop?.Invoke(this);
		}
	}
}
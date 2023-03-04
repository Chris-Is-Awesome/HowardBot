using AutoHotkey.Interop;

namespace HowardBot.Rewards
{
	public class InputEffect : RewardEffect
	{
		public delegate void EffectFunc(string userInput);

		private readonly AutoHotkeyEngine ahk;

		public EffectFunc StartFunc { get { return Start; } }

		public InputEffect(RewardHandler.RewardData.Reward rewardData) : base(rewardData)
		{
			ahk = Bot.AHK;
		}

		private void Start(string userInput)
		{
			Stop();
		}

		private void Stop()
		{
			onEffectStop?.Invoke(this);
		}
	}
}
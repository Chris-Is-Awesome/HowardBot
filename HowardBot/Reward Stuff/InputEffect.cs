using AutoHotkey.Interop;

namespace HowardBot
{
	class InputEffect : RewardEffect
	{
		public delegate void EffectFunc(string userInput);

		private readonly AutoHotkeyEngine ahk;

		public EffectFunc StartFunc { get { return Start; } }

		public InputEffect(string name, string rewardId) : base(name, rewardId)
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
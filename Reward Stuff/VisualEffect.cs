using System.Threading.Tasks;
using AutoHotkey.Interop;

namespace HowardBot
{
	class VisualEffect : RewardEffect
	{
		public delegate Task EffectFunc();

		private readonly AutoHotkeyEngine ahk;

		public int HotkeyNum { get; }
		public float Duration { get; }
		public EffectFunc StartFunc { get { return Start; } }

		public VisualEffect(string name, string rewardId, int hotkeyNum, float duration = 60) : base(name, rewardId)
		{
			HotkeyNum = hotkeyNum;
			Duration = duration;

			ahk = Bot.AHK;
		}

		private async Task Start()
		{
			await Utility.WaitForSeconds(1);

			ahk.ExecRaw($"Send {{Ctrl down}} {{0 down}} {{{HotkeyNum} down}}");
			await Utility.WaitForMilliseconds(10);
			ahk.ExecRaw($"Send {{Ctrl up}} {{0 up}} {{{HotkeyNum} up}}");

			await Utility.WaitForSeconds(Duration);
			await Stop();
		}

		private async Task Stop()
		{
			ahk.ExecRaw($"Send {{Ctrl down}} {{0 down}} {{{HotkeyNum} down}}");
			await Utility.WaitForMilliseconds(10);
			ahk.ExecRaw($"Send {{Ctrl up}} {{0 up}} {{{HotkeyNum} up}}");

			onEffectStop?.Invoke(this);
		}
	}
}
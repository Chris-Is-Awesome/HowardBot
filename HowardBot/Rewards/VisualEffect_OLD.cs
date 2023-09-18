using System.Threading.Tasks;
using AutoHotkey.Interop;

namespace HowardBot.Rewards
{
	public class VisualEffect_OLD : RewardEffect_OLD
	{
		public delegate Task EffectFunc();

		private readonly AutoHotkeyEngine ahk;

		public int HotkeyNum { get; }
		public float Duration { get; }
		public EffectFunc StartFunc { get { return Start; } }

		public VisualEffect_OLD(RewardHandler_OLD.RewardData.Reward rewardData, EffectData effectData) : base(rewardData)
		{
			HotkeyNum = effectData.hotkeyNum;
			Duration = effectData.duration;

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

		public class EffectData
		{
			public readonly int hotkeyNum;
			public readonly float duration;

			public EffectData(int hotkeyNum, float duration = 60)
			{
				this.hotkeyNum = hotkeyNum;
				this.duration = duration;
			}
		}
	}
}
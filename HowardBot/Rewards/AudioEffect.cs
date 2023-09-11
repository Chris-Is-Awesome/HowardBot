using SoundType = HowardBot.AudioPlayer.SoundType;

namespace HowardBot.Rewards
{
	public class AudioEffect : RewardEffect
	{
		public readonly SoundType type;
		public readonly string name;
		public readonly bool random;

		private AudioPlayer player;

		public delegate void RandomSoundFunc(SoundType type);
		public delegate void SoundFunc(SoundType type, string name);

		public RandomSoundFunc StartRandomSoundFunc { get { return StartRandomSound; } }
		public SoundFunc StartSoundFunc { get { return StartSound; } }

		public AudioEffect(RewardHandler.RewardData.Reward rewardData, EffectData effectData) : base(rewardData)
		{
			player = Bot.AudioPlayer;
			player.OnStopped += Stop;
			type = effectData.type;
			name = effectData.name;
			random = effectData.random;
		}

		private void StartRandomSound(SoundType type)
		{
			player.PlayRandomSound(type);
		}

		private void StartSound(SoundType type, string name)
		{
			player.PlaySound(type, name);
		}

		private void Stop()
		{
			onEffectStop?.Invoke(this);
		}

		public class EffectData
		{
			public readonly SoundType type;
			public readonly string name;
			public readonly bool random;

			public EffectData(SoundType type, string name = "", bool random = false)
			{
				this.type = type;
				this.name = name;
				this.random = random;
			}
		}
	}
}
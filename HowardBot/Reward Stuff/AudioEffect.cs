using SoundType = HowardBot.AudioPlayer.SoundType;

namespace HowardBot
{
	class AudioEffect : RewardEffect
	{
		public readonly SoundType type;
		public readonly string soundName;
		public readonly bool random;

		private AudioPlayer player;

		public delegate void RandomSoundFunc(SoundType type);
		public delegate void SoundFunc(SoundType type, string name);

		public RandomSoundFunc StartRandomSoundFunc { get { return StartRandomSound; } }
		public SoundFunc StartSoundFunc { get { return StartSound; } }

		public AudioEffect(string name, string rewardId, SoundType type, string soundName = "", bool random = false) : base(name, rewardId)
		{
			player = AudioPlayer.Instance;
			player.OnStopped += Stop;
			this.type = type;
			this.soundName = soundName;
			this.random = random;
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
	}
}
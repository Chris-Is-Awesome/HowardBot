namespace HowardBot
{
	class AudioEffect : RewardEffect
	{
		public delegate void EffectFunc(SoundType type, bool random);

		public EffectFunc StartFunc { get { return Start; } }

		public enum SoundType
		{
			Sound,
			Song
		}

		public AudioEffect(string name, string rewardId) : base(name, rewardId) { }

		private void Start(SoundType type, bool random)
		{
			AudioPlayer player = AudioPlayer.Instance;

			if (type == SoundType.Sound)
				if (random)
					player.PlayRandomSound();
				//else
					//player.PlaySound();
			else
				if (random)
					player.PlayRandomSong();
				//else
					//player.PlaySong();
				
			player.OnStopped += Stop;
		}

		private void Stop()
		{
			onEffectStop?.Invoke(this);
		}
	}
}
namespace HowardBot.Rewards
{
	class AudioReward : CustomRewardEffect
	{
		private readonly AudioPlayer player;

		public string SoundName { get; init; }
		public AudioPlayer.SoundType SoundType { get; init; } = AudioPlayer.SoundType.SoundClip;
		public float Volume { get; init; } = 1;
		public bool Random { get; init; } = false;

		public AudioReward()
		{
			player = Bot.AudioPlayer;
		}

		protected override void StartEffect(string userInput)
		{
			if (Random)
				player.PlayRandomSound(SoundType, Volume);
			else
				player.PlaySound(SoundType, SoundName, Volume);

			player.OnStopped += OnEffectDone;
		}
	}
}
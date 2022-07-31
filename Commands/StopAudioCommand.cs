namespace HowardBot.Commands
{
	class StopAudioCommand : Command
	{
		public StopAudioCommand() { }

		public override string Run(string[] args)
		{
			AudioPlayer.Instance.StopAllSounds();
			return "Stopped all audio tracks.";
		}
	}
}
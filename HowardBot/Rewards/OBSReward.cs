namespace HowardBot.Rewards
{
	class OBSReward : CustomRewardEffect
	{
		private readonly OBSHandler obs;

		public string Filter { get; init; }
		public int Duration { get; init; } = 60;

		public OBSReward()
		{
			obs = Bot.OBSHandler;
		}

		protected override void StartEffect(string userInput)
		{
			Debug.Log($"Started OBS effect with duration of {Duration} seconds!");
			DoEffect();
		}

		private async void DoEffect()
		{
			await Utility.WaitForSeconds(3);
			OnEffectDone();
		}
	}
}
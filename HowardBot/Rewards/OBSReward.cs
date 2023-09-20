using System.Threading;

namespace HowardBot.Rewards
{
	class OBSReward : CustomRewardEffect
	{
		private readonly OBSHandler obs;
		private string randomFilter;

		public string Source { get; init; }
		public string Filter { get; set; }
		public int Duration { get; init; } = 10;

		public OBSReward()
		{
			obs = OBSHandler.Instance;
		}

		protected override void StartEffect(string userInput)
		{
			new Thread(() =>
			{
				ToggleFilter(true, userInput);
				string filterToggled = randomFilter != null ? randomFilter : Filter;
				TwitchHandler.SendMessage($"Activated {filterToggled}! Will last for {Duration} seconds.");

				Thread.Sleep(Duration * 1000);

				ToggleFilter(false, userInput);
				TwitchHandler.SendMessage($"Deactivated {filterToggled} effect.");
				OnEffectDone();
			}).Start();
		}

		private void ToggleFilter(bool enable, string filter)
		{
			if (!string.IsNullOrEmpty(filter))
				Filter = filter;

			if (Filter == "Random")
			{
				if (enable)
					randomFilter = obs.ToggleRandomFilter(Source).FilterName;
				else if (randomFilter != null)
				{
					obs.ToggleFilter(Source, randomFilter, false);
					randomFilter = null;
				}
			}
			else
				obs.ToggleFilter(Source, Filter, enable);
		}
	}
}
using System.Threading.Tasks;

namespace HowardBot.Commands
{
	class ToggleRewardsCommand : Command
	{
		public override async Task<string> RunAsync(string[] args)
		{
			if (args.Length > 0)
			{
				string arg = args[0].ToLower();

				if (arg == "enable" || arg == "e")
				{
					await RewardHandler.Instance.EnableCustomRewards(true);
					return "Custom rewards enabled!";
				}
				else if (arg == "disable" || arg == "d")
				{
					await RewardHandler.Instance.DisableCustomRewards();
					return "Custom rewards disabled!";
				}
			}

			return "Requires argument 0 to be 'enable' or 'disable' because I was too lazy to add a prop to RewardHandler to check enabled state.";
		}
	}
}
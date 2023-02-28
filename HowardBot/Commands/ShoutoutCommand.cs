using System.Linq;
using System.Threading.Tasks;

namespace HowardBot.Commands
{
	class ShoutoutCommand : Command
	{
		public override async Task<string> RunAsync(string[] args)
		{
			if (args.Length > 0 && args[0].Length > 1)
			{
				string arg0 = args[0];

				// If user name is given, get ID
				if (arg0.StartsWith('@'))
				{
					var name = arg0.Substring(1);
					var user = await API.Instance.GetUserByName(name);
					string output;

					if (user != null)
					{
						var channelInfo = await API.Instance.GetChannelInfo(user.Id);
						output = $"Let's give a round of applause to {name}!";

						// Add game
						if (!string.IsNullOrEmpty(channelInfo.GameName))
							output += $" They were last playing {channelInfo.GameName}!";
					}
					else
						output = $"No user with name '{name}' was found. A typo perhaps?";

					return output;
				}
				// If ID given
				else if (arg0.All(char.IsDigit))
				{
					var user = await API.Instance.GetUserByID(arg0);
					string output;

					if (user != null)
					{
						var channelInfo = await API.Instance.GetChannelInfo(user.Id);
						output = $"Let's give a round of applause to {user.DisplayName}!";

						// Add game
						if (!string.IsNullOrEmpty(channelInfo.GameName))
							output += $" They were last playing {channelInfo.GameName}!";
					}
					else
						output = $"No user with ID '{arg0}' was found. A typo perhaps?";

					return output;
				}
				// If invalid argument given
				else
					return $"Must either give '@user' or their ID.";
			}

			return "Must specify a user to shoutout!";
		}
	}
}
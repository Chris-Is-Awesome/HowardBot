using System.Linq;
using System.Threading.Tasks;

namespace HowardBot.Commands
{
	class ShoutoutCommand : Command
	{
		public ShoutoutCommand() { }

		public override async Task<string> RunAsync(string[] args)
		{
			// Parse @ChrisIsAwesome to channelId
			// Also accept direct channelId

			if (args != null)
			{
				string arg0 = args[0];

				// If user name is given, get ID
				if (arg0.StartsWith('@'))
				{
					var name = arg0.Substring(1);
					var id = await API.Instance.GetUserIdFromName(name);
					var game = await API.Instance.GetLastPlayedGameForUser(id);
					string output = $"Let's give a round of applause to {name}!";

					// Add game
					if (!string.IsNullOrEmpty(game))
						output += $"They were last playing {game}!";

					return output;
				}
				// If ID given
				else if (arg0.All(char.IsDigit))
				{
					var name = await API.Instance.GetUserNameFromId(arg0);
					var game = await API.Instance.GetLastPlayedGameForUser(arg0);
					string output = $"Let's give a round of applause to {name}!";

					// Add game
					if (!string.IsNullOrEmpty(game))
						output += $" They were last playing {game}!";

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
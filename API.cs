using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix;

namespace HowardBot
{
	class API
	{
		public API(string clientId, string token)
		{
			TwitchAPI api = new TwitchAPI();
			api.Settings.ClientId = clientId;
			api.Settings.AccessToken = token;
			helix = api.Helix;
		}

		private static API _instance;
		private readonly Helix helix;

		public static API Instance
		{
			get
			{
				if (_instance == null)
					_instance = new API(Bot.ClientId, Bot.HowardToken);

				return _instance;
			}
		}

		/// <summary>
		/// Checks if I'm currently live
		/// </summary>
		/// <returns>[bool] Am I live or not?</returns>
		public async Task<bool> AmILive()
		{
			var response = await helix.Streams.GetStreamsAsync(userIds: new List<string> { Bot.ChannelId });
			return response.Streams.Length > 0; // Endpoint returns null if not live
		}

		/// <summary>
		/// Gets a user's DisplayName from their <paramref name="userId"/>.
		/// </summary>
		/// <param name="userId">The ID for the user. Can use <see cref="GetUserIdFromName(string)"/> to get a user's ID from their DisplayName.</param>
		/// <returns>[string] The user's DisplayName or an empty string if no user with <paramref name="userId"/> was found.</returns>
		public async Task<string> GetUserNameFromId(string userId)
		{
			var response = await helix.Users.GetUsersAsync(new List<string> { userId });
			return response.Users[0].DisplayName;
		}

		/// <summary>
		/// Gets a user's ID from their <paramref name="name"/>.
		/// </summary>
		/// <param name="name">The DisplayName for the user. Can use <see cref="GetUserNameFromId(string)"/> to get a user's DisplayName from their ID.</param>
		/// <returns>[string] The user's ID or an empty string if no user with <paramref name="name"/> was found.</returns>
		public async Task<string> GetUserIdFromName(string name)
		{
			var response = await helix.Users.GetUsersAsync(null, new List<string> { name });
			return response.Users[0].Id;
		}

		/// <summary>
		/// Gets the name of the last played game on the user's channel.
		/// </summary>
		/// <param name="userId">The ID for the user. Can use <see cref="GetUserIdFromName(string)"/> to get a user's ID from their DisplayName.</param>
		/// <returns>[string] The name of the last played game on the user's channel or an empty string if the user has not played any game.</returns>
		public async Task<string> GetLastPlayedGameForUser(string userId)
		{
			var response = await helix.Channels.GetChannelInformationAsync(userId);
			return response.Data[0].GameName;
		}
	}
}
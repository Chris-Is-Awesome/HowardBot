using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

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

		#region Users

		/// <param name="userId">The ID for the user. Can use <see cref="GetUserByName(string)"/> to get a user's ID from their DisplayName.</param>
		/// <returns>[string] The user's DisplayName or an empty string if no user with <paramref name="userId"/> was found.</returns>
		public async Task<User> GetUserByID(string userId)
		{
			var response = await helix.Users.GetUsersAsync(new List<string> { userId });

			if (response.Users != null && response.Users.Length > 0)
				return response.Users[0];

			return null;
		}

		/// <param name="name">The DisplayName for the user. Can use <see cref="GetUserByID(string)"/> to get a user's DisplayName from their ID.</param>
		/// <returns>[User] The User object or null if no user with <paramref name="name"/> was found.</returns>
		public async Task<User> GetUserByName(string name)
		{
			var response = await helix.Users.GetUsersAsync(null, new List<string> { name });

			if (response.Users != null && response.Users.Length > 0)
				return response.Users[0];

			return null;
		}

		#endregion

		#region Streams

		/// <param name="userId">User ID for the streamer</param>
		/// <returns>[Stream] The Stream object for the user's current stream, null if they're not live.</returns>
		public async Task<Stream> GetStreamForUser(string userId)
		{
			var response = await helix.Streams.GetStreamsAsync(userIds: new List<string> { userId });

			if (response.Streams != null && response.Streams.Length > 0)
				return response.Streams[0];

			return null;
		}

		/// <returns>[bool] Am I live or not?</returns>
		public async Task<bool> AmILive()
		{
			var response = await helix.Streams.GetStreamsAsync(userIds: new List<string> { Bot.ChannelId });
			return response.Streams.Length > 0; // Endpoint returns null if not live
		}

		#endregion

		#region Channels

		/// <param name="userId">The ID for the user. Can use <see cref="GetUserByName(string)"/> to get a user's ID from their DisplayName.</param>
		/// <returns>[ChannelInformation] The ChannelInformation object for the user, or null if user not found.</returns>
		public async Task<ChannelInformation> GetChannelInfo(string userId)
		{
			var response = await helix.Channels.GetChannelInformationAsync(userId);

			if (response != null && response.Data.Length > 0)
				return response.Data[0];

			return null;
		}

		#endregion
	}
}
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Helix.Models.ChannelPoints;
using TwitchLib.Api.Helix.Models.ChannelPoints.CreateCustomReward;
using TwitchLib.Api.Helix.Models.Channels.GetChannelInformation;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Helix.Models.Users.GetUsers;

namespace HowardBot
{
	class API : Singleton<API>
	{
		private readonly Helix helix;

		// Refs
		private Stream myCurrentStream;
		private CustomReward[] myRewards;

		public API()
		{
			TwitchAPI api = new();

			api.Settings.ClientId = Bot.ClientId;
			api.Settings.AccessToken = Bot.HowardToken;
			helix = api.Helix;
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
			bool myChannel = userId == Bot.ChannelId;

			// If looking for my own channel, grab reference
			if (myChannel && myCurrentStream != null)
				return myCurrentStream;

			var response = await helix.Streams.GetStreamsAsync(userIds: new List<string> { userId });

			if (response.Streams != null && response.Streams.Length > 0)
			{
				// If my stream, save to local var
				if (myChannel) myCurrentStream = response.Streams[0];

				return response.Streams[0];
			}

			return null;
		}

		/// <param name="userId">The user ID for whom is streaming</param>
		/// <returns>[List[string]] List of tags for the stream by <paramref name="userId"/>; empty List if stream is not active.</returns>
		public async Task<List<string>> GetStreamTags(string userId)
		{
			List<string> tags = new List<string>();
			var response = await helix.Streams.GetStreamTagsAsync(userId, Bot.HowardToken);

			if (response.Data != null && response.Data.Length > 0)
			{
				// Get localized name for each tag
				for (int i = 0; i < response.Data.Length; i++)
					tags.Add(response.Data[i].LocalizationNames["en-us"]);

				return tags;
			}

			return tags;
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

		#region Channel Points

		/// <summary>
		/// Creates a custom channel point reward and pushes it to Twitch
		/// </summary>
		/// <param name="userId">The ID for the user. Can use <see cref="GetUserByName(string)"/></param>
		/// <param name="reward">The custom reward object with all the properties set</param>
		/// <returns>The new custom channel point reward</returns>
		public async Task<CustomReward[]> CreateCustomReward(string userId, Rewards.Reward reward)
		{
			CreateCustomRewardsRequest newRewardRequest = new()
			{
				Title = reward.Title,
				Prompt = reward.Description,
				Cost = reward.Cost,
				IsEnabled = reward.IsEnabled,
				BackgroundColor = reward.BackgroundColor,
				IsUserInputRequired = reward.IsTextRequired,
				IsGlobalCooldownEnabled = reward.GlobalCooldownSeconds > 0,
				GlobalCooldownSeconds = reward.GlobalCooldownSeconds,
				IsMaxPerStreamEnabled = reward.MaxRedemptionsPerStream > 0,
				MaxPerStream = reward.MaxRedemptionsPerStream,
				IsMaxPerUserPerStreamEnabled = reward.MaxRedemptionsPerUserPerStream > 0,
				MaxPerUserPerStream = reward.MaxRedemptionsPerUserPerStream,
				ShouldRedemptionsSkipRequestQueue = reward.SkipRequestQueue
			};

			var response = await helix.ChannelPoints.CreateCustomRewardsAsync(broadcasterId: userId, request: newRewardRequest, accessToken: Bot.PubsubToken);

			return response.Data;
		}

		/// <summary>
		/// Deletes the custom channel point reward from Twitch
		/// </summary>
		/// <param name="userId">The ID for the user. Can use <see cref="GetUserByName(string)"/></param>
		/// <param name="reward">The custom reward object with all the properties set</param>
		public async Task DeleteCustomReward(string userId, Rewards.Reward reward)
		{
			await helix.ChannelPoints.DeleteCustomRewardAsync(broadcasterId: userId, reward.Id, Bot.PubsubToken);
		}

		/// <param name="userId">The ID for the user. Can use <see cref="GetUserByName(string)"/> to get a user's ID from their DisplayName.</param>
		/// <returns>[CustomReward[]] A CustomReward array containing the custom rewards for the channel; or null if none found.</returns>
		public async Task<CustomReward[]> GetChannelPointRewards(string userId)
		{
			bool myChannel = userId == Bot.ChannelId;

			if (myChannel && myRewards != null)
				return myRewards;

			var response = await helix.ChannelPoints.GetCustomRewardAsync(broadcasterId: userId, accessToken: Bot.PubsubToken);

			if (response != null && response.Data.Length > 0)
			{
				if (myChannel)
					myRewards = response.Data;

				return response.Data;
			}

			return null;
		}

		#endregion
	}
}
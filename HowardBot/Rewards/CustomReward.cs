using System.Collections.Generic;
using System.Threading.Tasks;

namespace HowardBot.Rewards
{
	/// <summary>
	/// The custom reward object
	/// </summary>
	class CustomReward
	{
		private readonly string _title = "A reward has no title...";
		private readonly string _description = "A reward has no description...";
		private readonly string _id;
		private readonly int _cost = 100;
		private readonly string _backgroundColor = "#000000";
		private readonly bool _isInputRequired = false;
		private readonly int _globalCooldownSeconds = 60;
		private readonly int _maxRedemptionsPerStream = 0;
		private readonly int _maxRedemptionsPerUser = 0;
		private readonly bool _skipRequestQueue = true;

		private CustomRewardEffect _effect;

		public enum Type
		{
			None,
			Audio,
			OBS
		}

		/// <summary>
		/// The reward object on Twitch
		/// </summary>
		public TwitchLib.Api.Helix.Models.ChannelPoints.CustomReward TwitchReward { get; private set; }
		/// <summary>
		/// The title of the reward
		/// </summary>
		public string Title
		{
			get { return TwitchReward != null ? TwitchReward.Title : _title; }
			init { _title = value; }
		}
		/// <summary>
		/// The text of the reward (shown when in the reward's menu)/>
		/// </summary>
		public string Description
		{
			get { return TwitchReward != null ? TwitchReward.Prompt : _description; }
			init { _description = value; }
		}
		/// <summary>
		/// The unique ID for the reward as it is on Twitch
		/// </summary>
		public string Id
		{
			get { return TwitchReward != null ? TwitchReward.Id : _id; }
			init { _id = value; }
		}
		/// <summary>
		/// The channel point cost for the reward
		/// </summary>
		public int Cost
		{
			get { return TwitchReward != null ? TwitchReward.Cost : _cost; }
			init { _cost = value; }
		}
		/// <summary>
		/// Is the reward enabled on Twitch?
		/// </summary>
		public bool Enabled
		{
			get { return TwitchReward != null ? TwitchReward.IsEnabled : false; }
		}
		/// <summary>
		/// The background color (in hex) for the reward
		/// </summary>
		public string BackgroundColor
		{
			get { return TwitchReward != null ? TwitchReward.BackgroundColor : _backgroundColor; }
			init { _backgroundColor = value; }
		}
		/// <summary>
		/// Is input required from the user (do they have to submit the text field)?
		/// </summary>
		public bool IsInputRequired
		{
			get { return TwitchReward != null ? TwitchReward.IsUserInputRequired : _isInputRequired; }
			init { _isInputRequired = value; }
		}
		/// <summary>
		/// The time in seconds between each redemption by any user.
		/// </summary>
		public int GlobalCooldownSeconds
		{
			get { return TwitchReward != null ? TwitchReward.GlobalCooldownSetting.GlobalCooldownSeconds : _globalCooldownSeconds; }
			init { _globalCooldownSeconds = value; }
		}
		/// <summary>
		/// The maximum number of times this reward can be redeemed per stream, regardless of user
		/// </summary>
		public int MaxRedemptionsPerStream
		{
			get { return TwitchReward != null ? TwitchReward.MaxPerStreamSetting.MaxPerStream : _maxRedemptionsPerStream; }
			init { _maxRedemptionsPerStream = value; }
		}
		/// <summary>
		/// The maximum number of times this reward can be redeemed per user per stream
		/// </summary>
		public int MaxRedemptionsPerUser
		{
			get { return TwitchReward != null ? TwitchReward.MaxPerUserPerStreamSetting.MaxPerUserPerStream : _maxRedemptionsPerUser; }
			init { _maxRedemptionsPerUser = value; }
		}
		/// <summary>
		/// Should this reward skip the request queue?
		/// </summary>
		public bool SkipRequestQueue
		{
			get { return TwitchReward != null ? TwitchReward.ShouldRedemptionsSkipQueue : _skipRequestQueue; }
			init { _skipRequestQueue = value; }
		}
		/// <summary>
		/// Should this reward be disabled for non-interactive streams (eg. speedruns, science/dev streams, etc.)?
		/// </summary>
		public bool Interactive { get; init; } = false;
		/// <summary>
		/// Should this reward only be enabled for specific games? If so, it'll be enabled for the games in here
		/// </summary>
		public List<string> EnableForGames { get; init; }
		/// <summary>
		/// The type of the reward
		/// </summary>
		public Type RewardType { get; init; } = Type.None;
		/// <summary>
		/// The data required for this reward's effect to run
		/// </summary>
		public object Data { get; init; }

		/// <summary>
		/// The effect object for the reward
		/// </summary>
		public CustomRewardEffect Effect
		{
			get { return _effect; }
			set { _effect = value; }
		}

		public CustomReward() { }

		/// <summary>
		/// Adds this custom reward to Twitch
		/// </summary>
		/// <returns>Returns true if the custom reward was successfully added to Twitch; false otherwise</returns>
		public async Task<bool> AddToTwitch()
		{
			var response = await API.Instance.CreateCustomReward(Bot.ChannelId, this);

			if (response != null)
			{
				TwitchReward = response[0];
				return true;
			}

			return false;
		}

		/// <summary>
		/// Updates this custom reward on Twitch
		/// </summary>
		/// <returns>True if the custom reward was successfully updated on Twitch; false otherwise</returns>
		public async Task<bool> UpdateOnTwitch(string id, API.UpdateRewardRequest request)
		{
			var response = await API.Instance.UpdateCustomReward(Bot.ChannelId, id, request);

			if (response != null)
			{
				TwitchReward = response[0];
				return true;
			}

			return false;
		}

		/// <summary>
		/// Enables this reward on Twitch and stores the reference to the Twitch reward
		/// </summary>
		/// <param name="twitchReward">The reward object from Twitch</param>
		/// <returns>Returns true if the custom reward was successfully enabled; false otherwise</returns>
		public async Task<bool> Enable(TwitchLib.Api.Helix.Models.ChannelPoints.CustomReward twitchReward)
		{
			TwitchReward = twitchReward;
			var response = await API.Instance.ToggleCustomReward(Bot.ChannelId, twitchReward.Id, true);
			return response != null;
		}

		/// <summary>
		/// Disables the reward on Twitch
		/// </summary>
		/// <returns>Returns true if the custom reward was successfully disabled; false otherwise</returns>
		public async Task<bool> Disable()
		{
			var response = await API.Instance.ToggleCustomReward(Bot.ChannelId, TwitchReward.Id, false);
			return response != null;
		}

		public CustomReward Clone()
		{
			return (CustomReward)MemberwiseClone();
		}

		public readonly struct RewardJSONObject
		{
			public CustomReward[] Rewards { get; init; }
		}
	}
}
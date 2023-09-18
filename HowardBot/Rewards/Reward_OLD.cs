namespace HowardBot.Rewards
{
	/// <summary>
	/// Class for handling custom & existing channel point rewards
	/// </summary>
	public class Reward_OLD
	{
		private string _title;
		private string _description;
		private int _cost;
		private bool _isEnabled = true;
		private string _backgroundColor;
		private bool _isTextRequired;
		private int _globalCooldownSeconds;
		private int _maxRedemptionsPerStream;
		private int _maxRedemptionsPerUserPerStream;
		private bool _skipRequestQueue = true;

		#region Public setters

		/// <summary>
		/// Is this reward on Twitch?
		/// </summary>
		public bool OnTwitch { get; set; }

		/// <summary>
		/// The title of the reward
		/// </summary>
		public string Title
		{
			get
			{
				if (reward != null)
					return reward.Title;

				return _title;
			}
			set { _title = value; }
		}

		/// <summary>
		/// The description of the reward
		/// </summary>
		public string Description
		{
			get
			{
				if (reward != null)
					return reward.Prompt;

				return _description;
			}
			set { _description = value; }
		}

		/// <summary>
		/// The cost in channel points of the reward
		/// </summary>
		public int Cost
		{
			get
			{
				if (reward != null)
					return reward.Cost;

				return _cost;
			}
			set { _cost = value; }
		}

		/// <summary>
		/// Is the reward enabled? True by default
		/// </summary>
		public bool IsEnabled
		{
			get
			{
				if (reward != null)
					return reward.IsEnabled;

				return _isEnabled;
			}
			set { _isEnabled = value; }
		}

		/// <summary>
		/// The background color (in hex) for the image
		/// </summary>
		public string BackgroundColor
		{
			get
			{
				if (reward != null)
					return reward.BackgroundColor;

				return _backgroundColor;
			}
			set { _backgroundColor = value; }
		}

		/// <summary>
		/// Does the user need to enter text?
		/// </summary>
		public bool IsTextRequired
		{
			get
			{
				if (reward != null)
					return reward.IsUserInputRequired;

				return _isTextRequired;
			}
			set { _isTextRequired = value; }
		}

		/// <summary>
		/// The number of seconds that must pass after the reward is used before it can be used again by anyone
		/// </summary>
		public int GlobalCooldownSeconds
		{
			get
			{
				if (reward != null)
					return reward.GlobalCooldownSetting.GlobalCooldownSeconds;

				return _globalCooldownSeconds;
			}
			set { _globalCooldownSeconds = value; }
		}

		/// <summary>
		/// The maximum number of times this reward can be redeemed by everyone per stream
		/// </summary>
		public int MaxRedemptionsPerStream
		{
			get
			{
				if (reward != null)
					return reward.MaxPerStreamSetting.MaxPerStream;

				return _maxRedemptionsPerStream;
			}
			set { _maxRedemptionsPerStream = value; }
		}

		/// <summary>
		/// The maxmimum number of times this reward can be redeemed per user per stream
		/// </summary>
		public int MaxRedemptionsPerUserPerStream
		{
			get
			{
				if (reward != null)
					return reward.MaxPerUserPerStreamSetting.MaxPerUserPerStream;

				return _maxRedemptionsPerUserPerStream;
			}
			set { _maxRedemptionsPerUserPerStream = value; }
		}

		/// <summary>
		/// Should this reward skip the redemption queue? True by default
		/// </summary>
		public bool SkipRequestQueue
		{
			get
			{
				if (reward != null)
					return reward.ShouldRedemptionsSkipQueue;

				return _skipRequestQueue;
			}
			set { _skipRequestQueue = value; }
		}

		#endregion

		#region Private setters

		/// <summary>
		/// The unique ID of the reward
		/// </summary>
		public string Id
		{
			get
			{
				if (reward != null)
					return reward.Id;

				Debug.LogError("Tried to access property on reward before it's been defined.");
				return string.Empty;
			}
		}

		/// <summary>
		/// Is the reward paused from redemptions?
		/// </summary>
		public bool IsPaused
		{
			get
			{
				if (reward != null)
					return reward.IsPaused;

				Debug.LogError("Tried to access property on reward before it's been defined.");
				return false;
			}
		}

		/// <summary>
		/// Is the reward in stock?
		/// </summary>
		public bool IsInStock
		{
			get
			{
				if (reward != null)
					return reward.IsInStock;

				Debug.LogError("Tried to access property on reward before it's been defined.");
				return false;
			}
		}

		#endregion

		private TwitchLib.Api.Helix.Models.ChannelPoints.CustomReward reward;

		/// <summary>
		/// Adds the custom reward to Twitch
		/// </summary>
		public async void AddRewardToTwitch()
		{
			reward = null;
			await Utility.WaitForSeconds(0);
		}
	}
}
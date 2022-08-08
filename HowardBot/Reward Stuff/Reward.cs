namespace HowardBot.TwitchSucks
{
	class Reward
	{
		public string Title { get; }
		public string Id { get; }
		public int Cost { get; }
		public string Prompt { get; }

		public Reward(TwitchLib.Api.Helix.Models.ChannelPoints.CustomReward reward)
		{
			Title = reward.Title;
			Id = reward.Id;
			Cost = reward.Cost;
			Prompt = reward.Prompt;
		}

		public Reward(TwitchLib.PubSub.Models.Responses.Messages.Redemption.Reward reward)
		{
			Title = reward.Title;
			Id = reward.Id;
			Cost = reward.Cost;
			Prompt = reward.Prompt;
		}
	}
}
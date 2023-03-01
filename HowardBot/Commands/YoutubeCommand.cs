using System.Configuration;

namespace HowardBot.Commands
{
	class YoutubeCommand : Command
	{
		public YoutubeCommand()
		{
			mainChannelLink = ConfigurationManager.AppSettings["YOUTUBE_MAIN_LINK"];
			vodsChannelLink = ConfigurationManager.AppSettings["YOUTUBE_VODS_LINK"];
		}

		private readonly string mainChannelLink;
		private readonly string vodsChannelLink;

		public override string Run(string[] args)
		{
			return $"Want to see some low quality, mid-tier content when I'm not live? Binge my main YT! {mainChannelLink} FeelsOkayMan Want to see the VOD of a missed stream without using the Twitch player? Check out my VODs channel! {vodsChannelLink}";
		}
	}
}
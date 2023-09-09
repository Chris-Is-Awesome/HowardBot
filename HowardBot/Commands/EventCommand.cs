namespace HowardBot.Commands
{
	class EventCommand : Command
	{
		private readonly string info = "I'm doing a weekend of randomizers to support an animal sanctuary! For more info: http://bombch.us/DZz_ If you'd like to contribute, you can do so through Tiltify: https://donate.tiltify.com/@chris-is-awesome/best-friends";

		public override string Run(string[] args)
		{
			return info;
		}
	}
}
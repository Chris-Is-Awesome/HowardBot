namespace HowardBot.Commands
{
	class TestCommand : Command
	{
		public TestCommand() { }

		public override string Run(string[] args)
		{
			return "test";
		}
	}
}
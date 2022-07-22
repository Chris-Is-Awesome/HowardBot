namespace HowardPlays
{
	interface ICommand
	{
		public string Run(string[] args, string userName);
	}
}
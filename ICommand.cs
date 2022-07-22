namespace HowardPlays
{
	interface ICommand
	{
		public void Run(string[] args, string userName);
	}
}
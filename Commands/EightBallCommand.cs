using System.Linq;

namespace HowardBot.Commands
{
	class EightBallCommand : Command
	{
		public EightBallCommand()
		{
			answers = new string[]
			{
				// Yes
				"It is certain",
				"It is decidedly so",
				"Without a doubt",
				"Yes - definitely",
				"You may rely on it",
				"As I see it, yes",
				"Most likely",
				"Outlook good",
				"Yes",
				"Signs point to yes",
				// Ignore
				"Concentrate and ask again",
				// No
				"Don't count on it",
				"My reply is no",
				"My sources say no",
				"Outlook not so good"
			};
		}

		private readonly string[] answers;

		public override string Run(string[] args)
		{
			if (args.Length > 0)
			{
				bool allUpper = true;

				foreach (char c in string.Join(' ', args))
				{
					if (char.IsLetter(c) && char.IsLower(c))
					{
						allUpper = false;
						break;
					}
				}

				if (allUpper)
					return "/me Don't yell at me please, I'm just a ball";

				string answer = answers[Utility.GetRandomNumberInRange(0, answers.Length - 1)];
				return $"/me {answer}";
			}

			return "/me I don't know how to answer that";
		}
	}
}
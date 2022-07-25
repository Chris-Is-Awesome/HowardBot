﻿using System;

namespace HowardBot.Commands
{
	class DiscordCommand : Command
	{
		public DiscordCommand()
		{
			DotNetEnv.Env.Load();
			inviteLink = Environment.GetEnvironmentVariable("DISCORD_INVITE_LINK");
		}

		private readonly string inviteLink;

		public override string Run(string[] args)
		{
			return $"Got no friends? Fear not, fellow viewer! Join my Discord server and find all sorts of people who have no friends! FeelsOkayMan {inviteLink}";
		}
	}
}
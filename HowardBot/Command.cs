﻿using System.Threading.Tasks;

namespace HowardBot
{
	class Command
	{
		private bool _enabled = true;

		public bool Enabled
		{
			get { return _enabled; }
			set { _enabled = value; }
		}

		/// <summary>
		/// Runs the command.
		/// </summary>
		/// <param name="args">The arguments for the command</param>
		/// <returns>[string] The message to send to chat after command executes.</returns>
		public virtual string Run(string[] args)
		{
			return string.Empty;
		}

		/// <summary>
		/// Runs the command asynchronously.
		/// </summary>
		/// <param name="args">The arguments for the command</param>
		/// <returns>[string] The message to send to chat after command executes.</returns>
		public virtual async Task<string> RunAsync(string[] args)
		{
			return await Task.FromResult(string.Empty);
		}
	}
}
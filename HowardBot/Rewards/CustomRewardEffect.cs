namespace HowardBot.Rewards
{
	abstract class CustomRewardEffect
	{
		public bool IsActive { get; protected set; }

		public delegate void OnEffectFinishedFunc(CustomRewardEffect effect);
		public event OnEffectFinishedFunc OnEffectFinished;

		/// <summary>
		/// Starts the effect
		/// </summary>
		/// <param name="userInput">The user input receieved from redemption</param>
		protected abstract void StartEffect(string userInput);

		public void TriggerEffect(string userInput)
		{
			StartEffect(userInput);
			IsActive = true;
		}

		protected virtual void OnEffectDone()
		{
			IsActive = false;
			OnEffectFinished?.Invoke(this);
		}
	}
}
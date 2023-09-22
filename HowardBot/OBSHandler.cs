using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using OBSStudioClient;
using OBSStudioClient.Classes;
using OBSStudioClient.Enums;

namespace HowardBot
{
	class OBSHandler : Singleton<OBSHandler>
	{
		private readonly ObsClient obs;
		private List<GameFilter> gameFilters = new();
		private bool _connected;

		public bool IsConnected { get { return _connected; } }

		public OBSHandler()
		{
			obs = new ObsClient();
			obs.PropertyChanged += OnPropertyChanged;
			obs.CurrentProgramSceneChanged += OnSceneChanged;
		}

		/// <summary>
		/// Cnnects to the OBS WebSocket server with the given arguments
		/// </summary>
		/// <param name="args">The connection arguments</param>
		/// <returns>True if the connection & authentication was successful, false otherwise</returns>
		public async Task<bool> Connect(ConnectionArgs args)
		{
			await obs.ConnectAsync(hostname: args.Host, port: args.Port, password: args.Password);
			await Utility.WaitForMilliseconds(1);
			return _connected;
		}

		/// <summary>
		/// Disconnects from the OBS WebSocket server
		/// </summary>
		public void Disconnect()
		{
			OnDisconnecting();
			obs.Disconnect();
		}

		public async void ToggleFilter(string source, string filter, bool enable)
		{
			if (enable)
				await SetFilterSettings(source, filter);

			// If toggling Howard
			if (filter == "Howard DVD")
			{
				int id = await obs.GetSceneItemId("Gaming", "HowardDVD");
				await obs.SetSceneItemEnabled("Gaming", id, enable);
			}
			else
				await obs.SetSourceFilterEnabled(source, filter, enable);

			GameFilter gameFilter = gameFilters.Find(x => x.filter.FilterName == filter);
			int index = gameFilters.IndexOf(gameFilter);
			gameFilter.isActive = enable;
			gameFilters[index] = gameFilter;
		}

		public void ToggleFilter(string source, Filter filter, bool enable)
		{
			obs.SetSourceFilterEnabled(source, filter.FilterName, enable);
		}

		public Filter ToggleRandomFilter(string source)
		{
			List<GameFilter> inactiveFilters = gameFilters.FindAll(x => !x.isActive);

			if (inactiveFilters.Count > 0)
			{
				Filter randomFilter = inactiveFilters[Utility.GetRandomNumberInRange(0, inactiveFilters.Count - 1)].filter;
				ToggleFilter(source, randomFilter.FilterName, true);
				return randomFilter;
			}

			return null;
		}

		private async Task SetFilterSettings(string source, string filter)
		{
			// Set random rotation
			if (filter == "Rotate")
			{
				// Reset rotation
				Dictionary<string, object> defaultSettings = new()
				{
					{ "rot_x", 0 },
					{ "rot_y", 0 },
					{ "rot_z", 0 }
				};
				await obs.SetSourceFilterSettings(source, filter, defaultSettings);

				// Create settings object
				Dictionary<string, object> settings = new()
				{
					{ "rot_x", Utility.GetRandomNumberInRange(-50, 50) },
					{ "rot_y", Utility.GetRandomNumberInRange(-50, 50) },
					{ "rot_z", Utility.GetRandomNumberInRange(-120, 120) }
				};

				// Set rotation
				await obs.SetSourceFilterSettings(source, filter, settings);
			}
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(obs.ConnectionState))
			{
				bool authSuccess = false;

				// If authentication was successful
				if (obs.ConnectionState == ConnectionState.Connected)
					authSuccess = true;
				// If disconnected
				else if (obs.ConnectionState == ConnectionState.Disconnected)
				{
					// If authentication was unsuccessful
					if (!authSuccess && !_connected && !Bot.TestLiveStuff)
						Debug.LogWarning($"Couldn't connect to OBS Websocket — Authentication failed.\nDouble check the following:\n1. Password and port are correct, and host should be 'localhost',\n2. WebSocket server is enabled, and\n3. OBS is running");

					_connected = false;
				}
				else if (obs.ConnectionState == ConnectionState.Disconnecting)
					OnDisconnecting();

				if (authSuccess)
					OnConnected();
			}
		}

		private async Task OnSceneChanged(object sender, OBSStudioClient.Events.SceneNameEventArgs e)
		{
			if (Bot.AmILive)
			{
				if (e.SceneName == "Gaming")
					await RewardHandler.Instance.EnableCustomRewards();
				else
					await RewardHandler.Instance.DisableCustomRewards();
			}
		}

		private void OnConnected()
		{
			_connected = true;
			GetGameFilters();
		}

		private void OnDisconnecting()
		{

		}

		private async void GetGameFilters()
		{
			Filter[] filters = await obs.GetSourceFilterList("Games");

			for (int i = 0; i < filters.Length; i++)
				gameFilters.Add(new GameFilter(filters[i]));
		}

		public struct ConnectionArgs
		{
			/// <summary>
			/// Should be "localhost" unless I'm using a custom server
			/// </summary>
			public string Host { get; set; }
			/// <summary>
			/// Must be the same as what's in OBS
			/// </summary>
			public int Port { get; set; }
			/// <summary>
			/// Must be the same as what's in OBS
			/// </summary>
			public string Password { get; set; }
		}

		private struct GameFilter
		{
			public Filter filter;
			public bool isActive;
			public bool IsActive { get; set; } = true;

			public GameFilter(Filter filter, bool isActive = false)
			{
				this.filter = filter;
				this.isActive = isActive;
			}
		}
	}
}
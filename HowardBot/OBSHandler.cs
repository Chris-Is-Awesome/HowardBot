using System.ComponentModel;
using System.Threading.Tasks;
using OBSStudioClient;
using OBSStudioClient.Enums;

namespace HowardBot
{
	class OBSHandler : Singleton<OBSHandler>
	{
		private readonly ObsClient obs;
		private bool connected;

		public OBSHandler()
		{
			obs = new ObsClient();
			obs.PropertyChanged += OnPropertyChanged;
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
			return connected;
		}

		/// <summary>
		/// Disconnects from the OBS WebSocket server
		/// </summary>
		public void Disconnect()
		{
			OnDisconnecting();
			obs.Disconnect();
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
					if (!authSuccess && !connected && !Bot.TestLiveStuff)
						Debug.LogWarning($"Couldn't connect to OBS Websocket — Authentication failed.\nDouble check the following:\n1. Password and port are correct, and host should be 'localhost',\n2. WebSocket server is enabled, and\n3. OBS is running");

					connected = false;
				}
				else if (obs.ConnectionState == ConnectionState.Disconnecting)
					OnDisconnecting();

				if (authSuccess)
					OnConnected();
			}
		}

		private void OnConnected()
		{
			connected = true;
			//obs.SetSourceFilterEnabled("Backgrounds", "test", true);
		}

		private void OnDisconnecting()
		{
			//obs.SetSourceFilterEnabled("Backgrounds", "test", false);
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
	}
}
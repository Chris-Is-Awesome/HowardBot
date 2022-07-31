using NAudio.Wave;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace HowardBot
{
	class AudioPlayer
	{
		public AudioPlayer()
		{
			_instance = this;
			allSongs = CreateSoundObjects(musicDir);
			allSounds = CreateSoundObjects(soundsDir);
		}

		private static AudioPlayer _instance;

		private readonly List<SoundData> allSongs;
		private readonly List<SoundData> allSounds;
		private const string musicDir = @".\Audio\Music\";
		private const string soundsDir = @".\Audio\Sounds\";

		public static AudioPlayer Instance
		{
			get
			{
				if (_instance == null)
					_instance = new AudioPlayer();

				return _instance;
			}
		}

		public void PlaySong(string songName)
		{
			PlaySound(GetSong(songName));
		}

		public void PlaySound(string soundName)
		{
			PlaySound(GetSound(soundName));
		}

		public void PlayRandomSong()
		{
			if (allSongs == null || allSongs.Count < 1)
			{
				Debug.LogError($"Tried to play a random song, but there are no songs in the directory '{musicDir}'");
				return;
			}

			SoundData sound = allSongs[Utility.GetRandomNumberInRange(0, allSongs.Count - 1)];
			PlaySound(sound);
		}

		public void PlayRandomSound()
		{
			if (allSounds == null || allSounds.Count < 1)
			{
				Debug.LogError($"Tried to play a random sound, but there are no sounds in the directory '{soundsDir}'");
				return;
			}

			SoundData sound = allSounds[Utility.GetRandomNumberInRange(0, allSounds.Count - 1)];
			PlaySound(sound);
		}

		private List<SoundData> CreateSoundObjects(string dir)
		{
			List<SoundData> sounds = new List<SoundData>();
			string[] files = Directory.GetFiles(dir, "", SearchOption.AllDirectories);

			foreach (string file in files)
			{
				string name = file.Substring(file.LastIndexOf('\\') + 1, (file.LastIndexOf('.') - file.LastIndexOf('\\')) - 1);
				sounds.Add(new SoundData(name, file, 1));
			}

			return sounds;
		}

		private SoundData GetSong(string songName)
		{
			if (allSongs == null || allSongs.Count < 1)
			{
				Debug.LogError($"Tried to get a song, but there are no songs in the directory '{musicDir}'");
				return null;
			}

			return allSongs.Find(x => x.name == songName);
		}

		private SoundData GetSound(string soundName)
		{
			if (allSounds == null || allSounds.Count < 1)
			{
				Debug.LogError($"Tried to get a sound, but there are no sounds in the directory '{soundsDir}'");
				return null;
			}

			return allSounds.Find(x => x.name == soundName);
		}

		private void PlaySound(SoundData sound)
		{
			new Thread(() =>
			{
				using (var audioFile = new AudioFileReader(sound.path))
				{
					using (var outputDevice = new WaveOutEvent())
					{
						outputDevice.Init(audioFile);
						outputDevice.Volume = sound.volume;
						outputDevice.Play();

						// Keep sound playing for its duration
						while (outputDevice.PlaybackState == PlaybackState.Playing)
						{
							Thread.Sleep(audioFile.TotalTime);
						}
					}
				}
			}).Start();
		}

		private class SoundData
		{
			public readonly string name;
			public readonly string path;
			public readonly float volume;

			public SoundData(string name, string path, float volume = 1)
			{
				this.name = name;
				this.path = path;
				this.volume = volume;
			}
		}
	}
}
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

		private List<WaveOutEvent> activeAudioOutputs = new List<WaveOutEvent>();

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
			PlaySound(GetSong(songName.ToLower()));
		}

		public void PlaySound(string soundName)
		{
			PlaySound(GetSound(soundName.ToLower()));
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

		public void StopAllSounds()
		{
			activeAudioOutputs.ForEach(x => x.Stop());
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

			SoundData song = allSongs.Find(x => x.name == songName);

			if (song == null) Debug.LogError($"No sound with name '{songName}' was found. A typo perhaps?", false);
			return song;
		}

		private SoundData GetSound(string soundName)
		{
			if (allSounds == null || allSounds.Count < 1)
			{
				Debug.LogError($"Tried to get a sound, but there are no sounds in the directory '{soundsDir}'");
				return null;
			}

			SoundData sound = allSounds.Find(x => x.name == soundName);

			if (sound == null) Debug.LogError($"No sound with name '{soundName}' was found. A typo perhaps?", false);
			return sound;
		}

		private void PlaySound(SoundData sound)
		{
			if (sound != null)
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
							activeAudioOutputs.Add(outputDevice);

							// Keep sound playing for its duration
							while (outputDevice.PlaybackState == PlaybackState.Playing)
							{
								Thread.Sleep(audioFile.TotalTime);
								activeAudioOutputs.Remove(outputDevice);
							}
						}
					}
				}).Start();
			}
		}

		private class SoundData
		{
			public readonly string name;
			public readonly string path;
			public readonly float volume;

			public SoundData(string name, string path, float volume = 1)
			{
				this.name = name.ToLower();
				this.path = path;
				this.volume = volume;
			}
		}
	}
}
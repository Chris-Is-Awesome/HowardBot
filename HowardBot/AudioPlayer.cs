using System.Collections.Generic;
using System.IO;
using System.Threading;
using NAudio.Wave;

namespace HowardBot
{
	public class AudioPlayer : Singleton<AudioPlayer>
	{
		public delegate void Func();

		public event Func OnStopped;

		private const string songsDir = @".\HowardBot\Audio\Songs";
		private const string soundClipsDir = @".\HowardBot\Audio\Sound Clips";
		private const string voiceClipsDir = @".\HowardBot\Audio\Voice Clips";
		private readonly List<SoundData> allSongs;
		private readonly List<SoundData> allSoundClips;
		private readonly List<SoundData> allVoiceClips;

		private List<WaveOutEvent> activeAudioOutputs = new();

		public AudioPlayer()
		{
			allSongs = CreateSoundObjects(songsDir);
			allSoundClips = CreateSoundObjects(soundClipsDir);
			allVoiceClips = CreateSoundObjects(voiceClipsDir);
		}

		public enum SoundType
		{
			Song,
			SoundClip,
			VoiceClip
		}

		public void PlayRandomSound(SoundType type, float volume)
		{
			List<SoundData> allSounds = GetSoundsList(type);
			int randIndex = Utility.GetRandomNumberInRange(0, allSounds.Count - 1);
			PlaySound(allSounds[randIndex], volume);
		}

		public void PlaySound(SoundType type, string name, float volume)
		{
			if (TryGetSound(type, name, out SoundData sound))
				PlaySound(sound, volume);
		}

		public void StopAllSounds()
		{
			activeAudioOutputs.ForEach(x => Stop(x));
		}

		private List<SoundData> CreateSoundObjects(string dir)
		{
			List<SoundData> sounds = new();
			string[] files = Directory.GetFiles(dir, "", SearchOption.AllDirectories);

			foreach (string file in files)
			{
				string name = file.Substring(file.LastIndexOf('\\') + 1, (file.LastIndexOf('.') - file.LastIndexOf('\\')) - 1);
				sounds.Add(new SoundData(name, file));
			}

			return sounds;
		}

		private bool TryGetSound(SoundType type, string name, out SoundData sound)
		{
			List<SoundData> allSounds = GetSoundsList(type);
			sound = allSounds.Find(x => x.name == name);

			if (sound == null)
			{
				Debug.LogError($"No sound of type '{type}' with name '{name}' was found");
				return false;
			}

			return true;
		}

		private List<SoundData> GetSoundsList(SoundType type)
		{
			return type switch
			{
				SoundType.Song => allSongs,
				SoundType.SoundClip => allSoundClips,
				SoundType.VoiceClip => allVoiceClips,
				_ => null,
			};
		}

		private void PlaySound(SoundData sound, float volume)
		{
			if (sound != null)
			{
				AudioFileReader audioFile = null;
				WaveOutEvent outputDevice = null;

				new Thread(() =>
				{
					using (audioFile = new AudioFileReader(sound.path))
					{
						using (outputDevice = new WaveOutEvent())
						{
							outputDevice.Init(audioFile);
							outputDevice.Volume = volume / 2;
							outputDevice.Play();
							activeAudioOutputs.Add(outputDevice);

							Thread.Sleep(audioFile.TotalTime);
							activeAudioOutputs.Remove(outputDevice);
							OnStopped?.Invoke();
						}
					}
				}).Start();
			}
		}

		private void Stop(WaveOutEvent output)
		{
			output.Stop();
			OnStopped?.Invoke();
		}

		private class SoundData
		{
			public readonly string name;
			public readonly string path;

			public SoundData(string name, string path)
			{
				this.name = name.ToLower();
				this.path = path;
			}
		}
	}
}
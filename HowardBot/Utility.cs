using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace HowardBot
{
	public static class Utility
	{
		public static string Timestamp
		{
			get
			{
				return $"[{DateTime.Now:hh:mm:ss.fff}]";
			}
		}

		public static string CurrentDirectory
		{
			get
			{
				return Directory.GetCurrentDirectory();
			}
		}

		/// <summary>
		/// Returns a random number between <paramref name="min"/> (inclusive) and <paramref name="max"/> (inclusive by default).
		/// </summary>
		/// <param name="min">The minimum number (inclusive)</param>
		/// <param name="max">The maximum number (exclusive)</param>
		/// <param name="maxInclusive">Should the maximum number be inclusive?</param>
		/// <returns>[int] The random number</returns>
		public static int GetRandomNumberInRange(int min, int max, bool maxInclusive = true)
		{
			Random rand = new();
			return rand.Next(min, maxInclusive ? max + 1 : max);
		}

		/// <summary>
		/// Asynchronously delays execution for <paramref name="seconds"/>.
		/// </summary>
		/// <param name="seconds">The number of seconds</param>
		public static async Task WaitForSeconds(float seconds)
		{
			await Task.Delay(TimeSpan.FromSeconds(seconds));
		}

		/// <summary>
		/// Asynchronously delays execution for <paramref name="milliseconds"/>.
		/// </summary>
		/// <param name="milliseconds">The numer of milliseconds</param>
		public static async Task WaitForMilliseconds(float milliseconds)
		{
			await Task.Delay(TimeSpan.FromMilliseconds(milliseconds));
		}

		/// <summary>
		/// Converts JSON text from a JSON file to type <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T">The type to deserialize to</typeparam>
		/// <param name="filePath">The path for the file</param>
		/// <returns>[<typeparamref name="T"/>] The deserialized type</returns>
		public static T DeserializeJSON<T>(string filePath)
		{
			using (StreamReader file = File.OpenText(filePath))
			{
				JsonSerializer serializer = new();
				return (T)serializer.Deserialize(file, typeof(T));
			}
		}

		/// <summary>
		/// Returns the environment variable with the given name, or empty string if none is found.
		/// </summary>
		/// <param name="varName">The name of the environment variable to retrieve</param>
		public static string LoadEnvVar(string varName)
		{
			return ConfigurationManager.AppSettings[varName];
		}

		/// <summary>
		/// Writes text to a text file. It will create the file and directory if they don't exist.
		/// </summary>
		/// <param name="directoryPath">The path to the parent directory containing the file</param>
		/// <param name="fileName">The filename of the file, including extension</param>
		/// <param name="text">The text to put in the file</param>
		/// <param name="appendIfExists">Append text to the existing file instead of overriding the text</param>
		public static void WriteToFile(string directoryPath, string fileName, string text, bool appendIfExists = true)
		{
			// Create the directory if it doesn't exist
			if (!Directory.Exists(directoryPath))
				Directory.CreateDirectory(directoryPath);

			// Get full path
			string fullPath = Path.Combine(directoryPath, fileName);

			try
			{
				// Write to file
				using (StreamWriter writer = new(fullPath, appendIfExists))
				{
					writer.WriteLine(text);
				}
			}
			catch (Exception ex)
			{
				Debug.Log($"Something went wrong when writing to file at path '{fullPath}'\n{ex.Message}");
			}
		}
	}
}
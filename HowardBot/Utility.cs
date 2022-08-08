using Newtonsoft.Json;
using System;
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

		/// <summary>
		/// Returns a random number between <paramref name="min"/> (inclusive) and <paramref name="max"/> (inclusive by default).
		/// </summary>
		/// <param name="min">The minimum number (inclusive)</param>
		/// <param name="max">The maximum number (exclusive)</param>
		/// <param name="maxInclusive">Should the maximum number be inclusive?</param>
		/// <returns>[int] The random number</returns>
		public static int GetRandomNumberInRange(int min, int max, bool maxInclusive = true)
		{
			Random rand = new Random();
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
				JsonSerializer serializer = new JsonSerializer();
				return  (T)serializer.Deserialize(file, typeof(T));
			}
		}
	}
}
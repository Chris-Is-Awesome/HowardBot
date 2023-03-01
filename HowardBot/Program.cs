using System;

namespace HowardBot
{
    class Program
    {
        private static void Main()
        {
            Console.Title = "HowardBot";

            Debug.Log($"[Bot started at {DateTime.Now}]");

            // Initialize bot
            new Bot();

            // Keep persistent
            Console.ReadLine();
        }
    }
}

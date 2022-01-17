using System;

namespace HowardPlays
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"[Bot started at {DateTime.Now}]");
            Bot bot = new();
            Console.ReadLine();
        }
    }
}

﻿using System;

namespace HowardPlays
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.Log($"[Bot started at {DateTime.Now}]");
            new Bot();
            Console.ReadLine();
        }
    }
}

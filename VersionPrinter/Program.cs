﻿using System;

namespace FileDB
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"{ReleaseInformation.Version.Major}.{ReleaseInformation.Version.Minor}");
        }
    }
}

using System;

namespace VersionPrinter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"{Shared.Shared.Version.Major}.{Shared.Shared.Version.Minor}");
        }
    }
}

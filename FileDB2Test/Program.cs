using System;
using FileDB2Interface;

namespace FileDBTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new FileDB2Config()
            {
                Database = @"C:\Source\filedb2_db\test.db",
            };

            var handle = new FileDB2Handle(config);
            var persons = handle.GetPersons();
        }
    }
}

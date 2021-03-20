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
                Database = @"C:\Source\filedb2_db\filedb2.db",
                FilesRootDirectory = @"X:",
            };

            try
            {
                var handle = new FileDB2Handle(config);

                //var files = handle.GetFiles();
                //var persons = handle.GetPersons();
                //var locations = handle.GetLocations();
                //var tags = handle.GetTags();

                //var file = handle.GetFileById(10);
                //var person = handle.GetPersonById(10);
                //var location = handle.GetLocationById(10);
                //var tag = handle.GetTagById(10);

                //var foundFiles = handle.SearchFiles("jul");
                //var foundPersons = handle.SearchPersons("pontus");
                //var foundLocations = handle.SearchLocations("enhagsvägen");
                //var foundTags = handle.SearchTags("gokart");

                //var fsDirectories = handle.ListAllFilesystemDirectories();
                //var fsFiles = handle.ListAllFilesystemFiles();

                //var missingFiles = handle.GetFilesMissingInFilesystem();

                //var file = handle.GetFileById(20115);
                //handle.ParseFilesystemFileExif("x:/" + file.path, out var dateTaken, out var location);

                //handle.InsertPerson("A", "C", "test", "yyyy-mm-dd", 1000000);
                //handle.DeletePerson(89);

                var persons = handle.GetPersonsFromFile(8067);
            }
            catch (FileDB2Exception e)
            {
                Console.WriteLine("FileDB2 error: " + e.Message);
            }
        }
    }
}

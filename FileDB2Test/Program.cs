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
                FilesRootDirectory = @"X:",
            };

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

            //var fsDirectories = handle.ListAllDirectories();
            //var fsFiles = handle.ListAllFiles();

            //var missingFiles = handle.GetFilesMissingInFilesystem();

            var file = handle.GetFileById(20115);
            handle.ParseFilesystemFileExif("x:/" + file.path, out var dateTaken, out var location);
        }
    }
}

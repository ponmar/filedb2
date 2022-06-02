using FileDBInterface.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileDB.Export
{
    public class DatabaseExporter
    {
        private readonly string destinationDirectory;

        public DatabaseExporter(string destinationDirectory)
        {
            this.destinationDirectory = destinationDirectory;
        }

        public void Export(List<PersonModel> persons, List<LocationModel> locations, List<TagModel> tags, List<FilesModel> files)
        {
            var fileContent = new ExportedDatabaseFileFormat()
            {
                FileDBVersion = ReleaseInformation.VersionString,
                ExportDateTime = DateTime.Now,
                Persons = persons,
                Locations = locations,
                Tags = tags,
                Files = files,
            };

            // TODO: where to check that directory is empty?
            var filename = Path.Combine(destinationDirectory, "DatabaseExport.json");
            new DatabaseJsonExporter().Export(fileContent, filename);
        }
    }
}

using FileDBInterface.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileDB.Export
{
    public class SearchResultExporter
    {
        private readonly string destinationDirectory;
        private readonly string header;

        public SearchResultExporter(string destinationDirectory, string header)
        {
            this.destinationDirectory = destinationDirectory;
            this.header = header;
        }

        public void Export(List<FilesModel> files)
        {
            var data = GetExportedData(files);

            var jsonPath = Path.Combine(destinationDirectory, "data.json");
            var xmlPath = Path.Combine(destinationDirectory, "data.xml");
            var htmlPath = Path.Combine(destinationDirectory, "index.html");
            var m3uPath = Path.Combine(destinationDirectory, "playlist.m3u");

            new FilesExporter().Export(data, destinationDirectory);
            new FilesWithDataExporter().Export(data, destinationDirectory);
            new JsonExporter().Export(data, jsonPath);
            new XmlExporter().Export(data, xmlPath);
            new M3uExporter().Export(data, m3uPath);
            new HtmlExporter().Export(data, htmlPath);
        }

        private DataFileFormat GetExportedData(List<FilesModel> files)
        {
            var model = Model.Model.Instance;

            var exportedFiles = new List<ExportedFile>();
            var persons = new List<PersonModel>();
            var locations = new List<LocationModel>();
            var tags = new List<TagModel>();

            int index = 1;
            foreach (var file in files)
            {
                var filePersons = model.DbAccess.GetPersonsFromFile(file.Id);
                foreach (var person in filePersons)
                {
                    if (!persons.Any(x => x.Id == person.Id))
                    {
                        persons.Add(person);
                    }
                }

                var fileLocations = model.DbAccess.GetLocationsFromFile(file.Id);
                foreach (var location in fileLocations)
                {
                    if (!locations.Any(x => x.Id == location.Id))
                    {
                        locations.Add(location);
                    }
                }

                var fileTags = model.DbAccess.GetTagsFromFile(file.Id);
                foreach (var tag in fileTags)
                {
                    if (!tags.Any(x => x.Id == tag.Id))
                    {
                        tags.Add(tag);
                    }
                }

                exportedFiles.Add(new ExportedFile()
                {
                    Id = file.Id,
                    ExportedPath = $"files/{index}{Path.GetExtension(file.Path)}",
                    OriginalPath = file.Path,
                    Description = file.Description,
                    Datetime = file.Datetime,
                    Position = file.Position,
                    PersonIds = filePersons.Select(x => x.Id).ToList(),
                    LocationIds = fileLocations.Select(x => x.Id).ToList(),
                    TagIds = fileTags.Select(x => x.Id).ToList()
                });

                index++;
            }

            return new DataFileFormat()
            {
                Header = header,
                About = $"Exported with {Utils.ApplicationName} {ReleaseInformation.Version.Major}.{ReleaseInformation.Version.Minor} {DateTime.Now:yyyy-MM-dd HH:mm}",
                Files = exportedFiles,
                Persons = persons,
                Locations = locations,
                Tags = tags
            };
        }
    }
}

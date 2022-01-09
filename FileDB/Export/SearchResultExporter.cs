using FileDBInterface.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace FileDB.Export
{
    public class SearchResultExporter
    {
        private readonly string destinationDirectory;

        public SearchResultExporter(string destinationDirectory)
        {
            this.destinationDirectory = destinationDirectory;
        }

        public void Export(List<FilesModel> files)
        {
            var exportedData = GetExportedData(files);

            var baseFilename = Path.Combine(destinationDirectory, $"{Utils.ApplicationName}Export");

            WriteJson(exportedData, $"{baseFilename}.json");
            WriteXml(exportedData, $"{baseFilename}.xml");
            WriteHtml(exportedData, $"{baseFilename}.html");
        }

        private ExportedData GetExportedData(List<FilesModel> files)
        {
            var model = Model.Model.Instance;

            var exportedFiles = new List<ExportedFile>();
            var persons = new List<PersonModel>();
            var locations = new List<LocationModel>();
            var tags = new List<TagModel>();

            int index = 1;
            foreach (var file in files)
            {
                foreach (var person in model.DbAccess.GetPersonsFromFile(file.Id))
                {
                    if (!persons.Any(x => x.Id == person.Id))
                    {
                        persons.Add(person);
                    }
                }
                foreach (var location in model.DbAccess.GetLocationsFromFile(file.Id))
                {
                    if (!locations.Any(x => x.Id == location.Id))
                    {
                        locations.Add(location);
                    }
                }
                foreach (var tag in model.DbAccess.GetTagsFromFile(file.Id))
                {
                    if (!tags.Any(x => x.Id == tag.Id))
                    {
                        tags.Add(tag);
                    }
                }

                var sourcePath = model.FilesystemAccess.ToAbsolutePath(file.Path);
                var destFilename = $"{index}{Path.GetExtension(file.Path)}";
                var destPath = Path.Combine(destinationDirectory, destFilename);
                File.Copy(sourcePath, destPath);
                index++;

                exportedFiles.Add(new() { Id = file.Id, Path = destFilename, Description = file.Description, Datetime = file.Datetime, Position = file.Position, PersonIds = persons.Select(x => x.Id).ToList(), LocationIds = locations.Select(x => x.Id).ToList(), TagIds = tags.Select(x => x.Id).ToList() });
            }

            return new ExportedData()
            {
                Header = "My exported files", // TODO: set from input from user
                About = $"Exported with {Utils.ApplicationName} {ReleaseInformation.Version.Major}.{ReleaseInformation.Version.Minor} {DateTime.Now:yyyy-MM-dd HH:mm}",
                Files = exportedFiles,
                Persons = persons,
                Locations = locations,
                Tags = tags
            };
        }

        private void WriteJson(ExportedData data, string filename)
        {
            var json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filename, json);
        }

        private void WriteXml(ExportedData data, string filename)
        {
            var xmlSerializer = new XmlSerializer(data.GetType());
            using var xmlFileStream = new StreamWriter(filename);
            using var xmlWriter = XmlWriter.Create(xmlFileStream, new XmlWriterSettings { Indent = true });
            xmlSerializer.Serialize(xmlWriter, data);
        }

        private void WriteHtml(ExportedData data, string filename)
        {
            var documentBase =
@"<!DOCTYPE html>
<html>
<head>
<title>%HEADER%</title>
<style>
img {
  width: 100%;
}
.picture {
  margin-top: 20px;
  margin-bottom: 20px;
}
</style>
</head>
<body>
<h1>%HEADER%</h1>
%CONTENT%
<p>%ABOUT%</p>
</body>
</html>
";

            var pictureBase =
@"<div class=""picture"">
  <a href=""%PATH%""><img src=""%PATH%"" alt=""%PATH%""></a>
</div>
";

            string content = string.Empty;
            foreach (var file in data.Files)
            {
                var pictureHtml = pictureBase.Replace("%PATH%", file.Path);
                content += pictureHtml;
            }

            var html = documentBase.Replace("%HEADER%", data.Header).Replace("%ABOUT%", data.About).Replace("%CONTENT%", content);
            File.WriteAllText(filename, html);
        }
    }
}

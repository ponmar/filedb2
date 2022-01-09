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
        private readonly string header;

        public SearchResultExporter(string destinationDirectory, string header)
        {
            this.destinationDirectory = destinationDirectory;
            this.header = header;
        }

        public void Export(List<FilesModel> files)
        {
            var exportedData = GetExportedData(files);

            var jsonPath = Path.Combine(destinationDirectory, "data.json");
            var xmlPath = Path.Combine(destinationDirectory, "data.xml");
            var htmlPath = Path.Combine(destinationDirectory, "index.html");
            var m3uPath = Path.Combine(destinationDirectory, "playlist.m3u");

            WriteJson(exportedData, jsonPath);
            WriteXml(exportedData, xmlPath);
            WriteM3u(exportedData, m3uPath);
            WriteHtml(exportedData, htmlPath);
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

                var sourcePath = model.FilesystemAccess.ToAbsolutePath(file.Path);
                var destFilename = $"{index}{Path.GetExtension(file.Path)}";
                var destPath = Path.Combine(destinationDirectory, destFilename);
                File.Copy(sourcePath, destPath);
                index++;

                exportedFiles.Add(new ExportedFile()
                {
                    Id = file.Id,
                    ExportedPath = destFilename,
                    OriginalPath = file.Path,
                    Description = file.Description,
                    Datetime = file.Datetime,
                    Position = file.Position,
                    PersonIds = filePersons.Select(x => x.Id).ToList(),
                    LocationIds = fileLocations.Select(x => x.Id).ToList(),
                    TagIds = fileTags.Select(x => x.Id).ToList()
                });
            }

            return new ExportedData()
            {
                Header = header,
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

        private void WriteM3u(ExportedData data, string filename)
        {
            var m3uLinebreak = "\r\n";
            int duration = 10; // Note: VLC ignore the duration information when showing images in a playlist
            var content = $"#EXTM3U{m3uLinebreak}";
            content += $"#PLAYLIST:{data.Header}{m3uLinebreak}";
            foreach (var file in data.Files)
            {
                content += $"#EXTINF:{duration},{file.OriginalPath}{m3uLinebreak}";
                content += $"{file.ExportedPath}{m3uLinebreak}";
            }

            File.WriteAllText(filename, content);
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
  border-radius: 10px;
  background-color: lightgray;
  padding: 10px;
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
  %PICTURETEXT%
  <a href=""%PATH%""><img src=""%PATH%"" alt=""%PATH%""></a>
</div>
";

            string content = string.Empty;
            int index = 1;
            foreach (var file in data.Files)
            {
                var pictureDateText = string.Empty;
                if (file.Datetime != null)
                {
                    pictureDateText = $"{file.Datetime}";
                }

                var pictureDescription = string.Empty;
                if (file.Description != null)
                {
                    if (pictureDateText != string.Empty)
                    {
                        pictureDescription += ": ";
                    }
                    pictureDescription += $"{file.Description}";
                }

                var pictureText = $"<h2>[{index} / {data.Files.Count}] {pictureDateText}{pictureDescription}</h2>";

                if (file.PersonIds.Count > 0)
                {
                    var persons = file.PersonIds.Select(x => data.Persons.First(y => y.Id == x));
                    var personsStr = string.Join(", ", persons.Select(x => $"{x.Firstname} {x.Lastname}{Utils.GetPersonAgeInFileString(file.Datetime, x.DateOfBirth)}"));
                    pictureText += $"<p>&#128578; {personsStr}</p>";
                }

                if (file.LocationIds.Count > 0)
                {
                    var locations = file.LocationIds.Select(x => data.Locations.First(y => y.Id == x));
                    var locationsStr = string.Join(", ", locations.Select(x => x.Name));
                    pictureText += $"<p>&#127968; {locationsStr}</p>";
                }

                if (file.TagIds.Count > 0)
                {
                    var tags = file.TagIds.Select(x => data.Tags.First(y => y.Id == x));
                    var tagsStr = string.Join(", ", tags.Select(x => x.Name));
                    pictureText += $"<p>&#128278; {tagsStr}</p>";
                }

                var pictureHtml = pictureBase.Replace("%PATH%", file.ExportedPath).Replace("%PICTURETEXT%", pictureText);
                content += pictureHtml;

                index++;
            }

            var html = documentBase.Replace("%HEADER%", data.Header).Replace("%ABOUT%", data.About).Replace("%CONTENT%", content);
            File.WriteAllText(filename, html);
        }
    }
}

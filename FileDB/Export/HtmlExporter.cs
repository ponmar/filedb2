using FileDBInterface.DbAccess;
using System.IO;
using System.Linq;

namespace FileDB.Export
{
    public class HtmlExporter : IExporter
    {
        public void Export(DataFileFormat data, string filename)
        {
            var documentBase =
@"<!DOCTYPE html>
<html>
<head>
<title>%HEADER%</title>
<style>
.file {
  margin-top: 20px;
  margin-bottom: 20px;
  border-radius: 10px;
  background-color: lightgray;
  padding: 10px;
}
.picture {
  width: 100%;
  height: 800px;
  background-repeat: no-repeat;
  background-size: contain;
}
.index {
  border-radius: 5px;
  background-color: gray;
  padding: 3px;
}
</style>
</head>
<body>
<h1>%HEADER%</h1>
%CONTENT%
<p>%ABOUT%</p>
<p>The %APPLICATION_NAME% application can be downloaded <a href=""%APPLICATION_DOWNLOAD_URL%"">here</a>.</p>
</body>
</html>
";

            var pictureBase =
@"<div class=""file"">
  %PICTURETEXT%
  <a href=""%PATH%""><div class=""picture"" style=""background-image: url(%PATH%);""></div></a>
</div>
";

            string content = string.Empty;
            int index = 1;
            foreach (var file in data.Files)
            {
                var pictureDateText = string.Empty;
                if (file.Datetime != null)
                {
                    pictureDateText = $"{CreateExportedFileDatetime(file.Datetime)}";
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

                var pictureText = $@"<h2><span class=""index"">{index} / {data.Files.Count}</span> {pictureDateText}{pictureDescription}</h2>";

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

            var html = documentBase
                .Replace("%HEADER%", data.Header)
                .Replace("%ABOUT%", data.About)
                .Replace("%CONTENT%", content)
                .Replace("%APPLICATION_DOWNLOAD_URL%", data.ApplicationDownloadUrl)
                .Replace("%APPLICATION_NAME%", Utils.ApplicationName);

            File.WriteAllText(filename, html);
        }

        private string CreateExportedFileDatetime(string fileDatetime)
        {
            var datetime = DatabaseParsing.ParseFilesDatetime(fileDatetime);
            if (datetime == null)
            {
                return null;
            }

            // Note: when no time is available the string is used to avoid including time 00:00
            return fileDatetime.Contains('T') ? datetime.Value.ToString("yyyy-MM-dd HH:mm") : fileDatetime;
        }
    }
}

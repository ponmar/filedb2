using System.IO;
using System.Linq;

namespace FileDB.Export
{
    public class HtmlExporter : IExporter
    {
        public void Export(ExportedData data, string filename)
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

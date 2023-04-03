using FileDB.Model;
using FileDBShared.FileFormats;
using FileDBShared.Model;
using FileDBShared.Validators;
using System.IO;
using System.Linq;
using System.Web;

namespace FileDB.Export;

public class SearchResultHtmlExporter : ISearchResultExporter
{
    public void Export(SearchResultExport data, string destinationDirPath)
    {
        if (!Directory.Exists(destinationDirPath))
        {
            Directory.CreateDirectory(destinationDirPath);
        }

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

        var fileSyustemAccess = ServiceLocator.Resolve<IFilesystemAccessRepository>();

        string content = string.Empty;
        int index = 1;
        foreach (var file in data.Files)
        {
            var sourceFilePath = fileSyustemAccess.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
            var destinationFilename = Path.GetFileName(file.ExportedPath);
            var destFilePath = Path.Combine(destinationDirPath, destinationFilename);
            File.Copy(sourceFilePath, destFilePath);

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
                var htmlDescription = HttpUtility.HtmlEncode(file.Description);
                htmlDescription = htmlDescription.Replace(FileModelValidator.DescriptionLineEnding, "<br>");
                pictureDescription += htmlDescription;
            }

            var pictureText = $@"<h2><span class=""index"">{index} / {data.Files.Count}</span> {pictureDateText}{pictureDescription}</h2>";

            if (file.PersonIds.Count > 0)
            {
                var persons = file.PersonIds.Select(x => data.Persons.First(y => y.Id == x));
                var personStrings = persons.Select(x => $"{x.Firstname} {x.Lastname}{Utils.GetPersonAgeInFileString(file.Datetime, x.DateOfBirth)}").ToList();
                personStrings.Sort();
                var personsStr = string.Join(", ", personStrings);
                pictureText += $"<p>&#128578; {personsStr}</p>";
            }

            if (file.LocationIds.Count > 0)
            {
                var locations = file.LocationIds.Select(x => data.Locations.First(y => y.Id == x));
                var locationStrings = locations.Select(x => x.Name).ToList();
                locationStrings.Sort();
                var locationsStr = string.Join(", ", locationStrings);
                pictureText += $"<p>&#127968; {locationsStr}</p>";
            }

            if (file.TagIds.Count > 0)
            {
                var tags = file.TagIds.Select(x => data.Tags.First(y => y.Id == x));
                var tagStrings = tags.Select(x => x.Name).ToList();
                tagStrings.Sort();
                var tagsStr = string.Join(", ", tagStrings);
                pictureText += $"<p>&#128278; {tagsStr}</p>";
            }

            var pictureHtml = pictureBase.Replace("%PATH%", destinationFilename).Replace("%PICTURETEXT%", pictureText);
            content += pictureHtml;

            index++;
        }

        var html = documentBase
            .Replace("%HEADER%", HttpUtility.HtmlEncode(data.Header))
            .Replace("%ABOUT%", HttpUtility.HtmlEncode(data.About))
            .Replace("%CONTENT%", content)
            .Replace("%APPLICATION_DOWNLOAD_URL%", data.ApplicationDownloadUrl)
            .Replace("%APPLICATION_NAME%", Utils.ApplicationName);

        var htmlPath = Path.Combine(destinationDirPath, "index.html");
        File.WriteAllText(htmlPath, html);
    }

    public static string? CreateExportedFileDatetime(string fileDatetime)
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

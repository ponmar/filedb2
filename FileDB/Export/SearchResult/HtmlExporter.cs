using FileDB.Extensions;
using FileDB.Model;
using FileDBShared.FileFormats;
using FileDBShared.Model;
using FileDBShared.Validators;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Web;

namespace FileDB.Export.SearchResult;

public class HtmlExporter : ISearchResultExporter
{
    private readonly IFileSystem fileSystem;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;

    public HtmlExporter(IFileSystem fileSystem, IFilesystemAccessProvider filesystemAccessProvider)
    {
        this.fileSystem = fileSystem;
        this.filesystemAccessProvider = filesystemAccessProvider;
    }

    public void Export(SearchResultExport data, string destinationDirPath)
    {
        if (!fileSystem.Directory.Exists(destinationDirPath))
        {
            fileSystem.Directory.CreateDirectory(destinationDirPath);
        }

        var documentBase =
@"<!DOCTYPE html>
<html>
<head>
<title>%NAME%</title>
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
<h1>%NAME%</h1>
%CONTENT%
<p>%APPLICATION_NAME% %FILEDB_VERSION% %EXPORT_DATETIME% <a href=""%APPLICATION_PROJECT_URL%"">%APPLICATION_PROJECT_URL%</a></p>
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
            var sourceFilePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
            var destinationFilename = Path.GetFileName(file.ExportedPath);
            var destFilePath = Path.Combine(destinationDirPath, destinationFilename);
            fileSystem.File.Copy(sourceFilePath, destFilePath);

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
            .Replace("%NAME%", HttpUtility.HtmlEncode(data.Name))
            .Replace("%APPLICATION_NAME%", HttpUtility.HtmlEncode(Utils.ApplicationName))
            .Replace("%FILEDB_VERSION%", HttpUtility.HtmlEncode(data.FileDBVersion))
            .Replace("%EXPORT_DATETIME%", HttpUtility.HtmlEncode(data.ExportDateTime.ToDateAndTime()))
            .Replace("%CONTENT%", content)
            .Replace("%APPLICATION_PROJECT_URL%", data.ApplicationProjectUrl);

        var htmlPath = Path.Combine(destinationDirPath, "index.html");
        fileSystem.File.WriteAllText(htmlPath, html);
    }

    public static string? CreateExportedFileDatetime(string fileDatetime)
    {
        var datetime = DatabaseParsing.ParseFilesDatetime(fileDatetime);
        if (datetime == null)
        {
            return null;
        }

        // Note: when no time is available the string is used to avoid including time 00:00
        return fileDatetime.Contains('T') ? datetime.Value.ToDateAndTime() : fileDatetime;
    }
}

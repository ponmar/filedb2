using FileDB.Extensions;
using FileDB.Model;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using FileDBInterface.Validators;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Web;

namespace FileDB.Export.SearchResult;

public interface ISelfContainedHtmlExporter
{
    void Export(RichExportData data, string destinationFilePath);
}

public class SelfContainedHtmlExporter(IFileSystem fileSystem, IFilesystemAccessProvider filesystemAccessProvider, IConfigProvider configProvider) : ISelfContainedHtmlExporter
{
    public void Export(RichExportData data, string destinationFilePath)
    {
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
  max-height: 800px;
  object-fit: contain;
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

        string content = string.Empty;
        int index = 1;
        var existingFiles = data.Files.Where(x => fileSystem.File.Exists(filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(x.OriginalPath)));
        var existingFilesList = existingFiles.ToList();

        foreach (var file in existingFilesList)
        {
            var pictureText = BuildFileMetaText(data, file, index, existingFilesList.Count);

            string fileHtml;
            if (file.FileType == FileType.Picture)
            {
                var sourceFilePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
                var imageBytes = fileSystem.File.ReadAllBytes(sourceFilePath);
                var base64 = Convert.ToBase64String(imageBytes);
                var mime = GetImageMimeType(file.OriginalPath);
                var dataUri = $"data:{mime};base64,{base64}";

                fileHtml = $"""
<div class="file">
  {pictureText}
  <img class="picture" src="{dataUri}" alt="{HttpUtility.HtmlEncode(Path.GetFileName(file.OriginalPath))}"/>
</div>
""";
            }
            else
            {
                fileHtml = $"""
<div class="file">
  {pictureText}
  <p>{HttpUtility.HtmlEncode(Path.GetFileName(file.OriginalPath))}</p>
</div>
""";
            }

            content += fileHtml;
            index++;
        }

        var html = documentBase
            .Replace("%NAME%", HttpUtility.HtmlEncode(data.Name))
            .Replace("%APPLICATION_NAME%", HttpUtility.HtmlEncode(Utils.ApplicationName))
            .Replace("%FILEDB_VERSION%", HttpUtility.HtmlEncode(data.FileDBVersion))
            .Replace("%EXPORT_DATETIME%", HttpUtility.HtmlEncode(data.ExportDateTime.ToDateAndTime()))
            .Replace("%CONTENT%", content)
            .Replace("%APPLICATION_PROJECT_URL%", data.ApplicationProjectUrl);

        fileSystem.File.WriteAllText(destinationFilePath, html);
    }

    private string BuildFileMetaText(RichExportData data, ExportedFile file, int index, int total)
    {
        var pictureDateText = string.Empty;
        if (file.Datetime is not null)
        {
            pictureDateText = HtmlExporter.CreateExportedFileDatetime(file.Datetime) ?? string.Empty;
        }

        var pictureDescription = string.Empty;
        if (file.Description is not null)
        {
            if (pictureDateText != string.Empty)
            {
                pictureDescription += ": ";
            }
            var htmlDescription = HttpUtility.HtmlEncode(file.Description);
            htmlDescription = htmlDescription.Replace(FileModelValidator.DescriptionLineEnding, "<br>");
            pictureDescription += htmlDescription;
        }

        var text = $"""<h2><span class="index">{index} / {total}</span> {pictureDateText}{pictureDescription}</h2>""";

        if (file.PersonIds.Count > 0)
        {
            var fileDateTime = DatabaseParsing.ParseFilesDatetime(file.Datetime);
            var persons = data.Persons.Where(x => file.PersonIds.Contains(x.Id));
            var personParts = persons
                .Select(p => (
                    Display: FileTextOverlayCreator.GetPersonText(p, file.Datetime),
                    Tooltip: FileTextOverlayCreator.GetPersonDetailsText(p, fileDateTime)))
                .OrderBy(x => x.Display)
                .Select(x => $"<span title=\"{HttpUtility.HtmlEncode(x.Tooltip)}\">{HttpUtility.HtmlEncode(x.Display)}</span>");
            text += $"<p>&#128578; {string.Join(", ", personParts)}</p>";
        }

        if (file.LocationIds.Count > 0)
        {
            var locations = data.Locations.Where(x => file.LocationIds.Contains(x.Id)).OrderBy(x => x.Name);
            var locationParts = locations.Select(l =>
            {
                var link = Utils.CreatePositionLink(l.Position, configProvider.Config.LocationLink);
                return link is not null
                    ? $"<a href=\"{link}\">{HttpUtility.HtmlEncode(l.Name)}</a>"
                    : HttpUtility.HtmlEncode(l.Name);
            });
            text += $"<p>&#127968; {string.Join(", ", locationParts)}</p>";
        }

        if (file.TagIds.Count > 0)
        {
            var tags = data.Tags.Where(x => file.TagIds.Contains(x.Id));
            var tagsStr = FileTextOverlayCreator.GetTagsText(tags, ", ");
            text += $"<p>&#128278; {tagsStr}</p>";
        }

        var filePositionLink = Utils.CreatePositionLink(file.Position, configProvider.Config.LocationLink);
        if (filePositionLink is not null)
        {
            text += $"<p>&#x1F6F0; <a href=\"{filePositionLink}\">{HttpUtility.HtmlEncode(file.Position)}</a></p>";
        }

        return text;
    }

    private static string GetImageMimeType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream",
        };
    }
}

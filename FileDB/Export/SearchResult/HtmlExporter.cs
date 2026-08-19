using FileDB.Extensions;
using FileDB.Model;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using FileDBInterface.Validators;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace FileDB.Export.SearchResult;

public interface IHtmlExporter
{
    void Export(RichExportData data, string destinationDirPath);
}

public class HtmlExporter(IFileSystem fileSystem, IFilesystemAccessProvider filesystemAccessProvider, IConfigProvider configProvider) : IHtmlExporter
{
    public void Export(RichExportData data, string destinationDirPath)
    {
        if (!fileSystem.Directory.Exists(destinationDirPath))
        {
            fileSystem.Directory.CreateDirectory(destinationDirPath);
        }

        var existingFiles = data.Files
            .Where(x => fileSystem.File.Exists(filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(x.OriginalPath)))
            .ToList();

        // Copy files and build JS slides array
        var slidesJs = new StringBuilder();
        slidesJs.AppendLine("const slides = [");
        for (int i = 0; i < existingFiles.Count; i++)
        {
            var file = existingFiles[i];
            var sourceFilePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
            var destinationFilename = Path.GetFileName(file.ExportedPath);
            var destFilePath = Path.Combine(destinationDirPath, destinationFilename);
            fileSystem.File.Copy(sourceFilePath, destFilePath);

            var metaHtml = HtmlExportUtils.BuildMetaHtml(data, file, configProvider.Config.LocationLink);
            var isPicture = file.FileType == FileType.Picture ? "true" : "false";
            slidesJs.AppendLine($"  {{ src: {ToJsString(destinationFilename)}, alt: {ToJsString(Path.GetFileName(file.OriginalPath))}, meta: {ToJsString(metaHtml)}, isPicture: {isPicture} }}{(i < existingFiles.Count - 1 ? "," : "")}");
        }
        slidesJs.AppendLine("];");

        var html = BuildHtmlDocument(data, slidesJs.ToString());
        var htmlPath = Path.Combine(destinationDirPath, "index.html");
        fileSystem.File.WriteAllText(htmlPath, html);
    }

    private static string ToJsString(string value) =>
        "`" + value.Replace("\\", "\\\\").Replace("`", "\\`").Replace("${", "\\${") + "`";

    private static string BuildHtmlDocument(RichExportData data, string slidesJs) =>
        HtmlExportUtils.BuildViewerHtml(data, slidesJs);

    public static string? CreateExportedFileDatetime(string fileDatetime)
    {
        var datetime = DatabaseParsing.ParseFilesDatetime(fileDatetime);
        if (datetime is null)
        {
            return null;
        }

        // Note: when no time is available the string is used to avoid including time 00:00
        return fileDatetime.Contains('T') ? datetime.Value.ToDateAndTime() : fileDatetime;
    }
}


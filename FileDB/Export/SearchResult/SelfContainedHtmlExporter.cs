using FileDB.Extensions;
using FileDB.Model;
using FileDB.Model.FileFormats;
using FileDBInterface.Model;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace FileDB.Export.SearchResult;

public interface ISelfContainedHtmlExporter
{
    void Export(RichExportData data, string destinationFilePath);
}

public class SelfContainedHtmlExporter(IFileSystem fileSystem, IFilesystemAccessProvider filesystemAccessProvider, IConfigProvider configProvider) : ISelfContainedHtmlExporter
{
    public void Export(RichExportData data, string destinationFilePath)
    {
        var existingFiles = data.Files
            .Where(x => fileSystem.File.Exists(filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(x.OriginalPath)))
            .ToList();

        // Build JS slides array with embedded base64 images
        var slidesJs = new StringBuilder();
        slidesJs.AppendLine("const slides = [");
        for (int i = 0; i < existingFiles.Count; i++)
        {
            var file = existingFiles[i];
            var metaHtml = HtmlExportUtils.BuildMetaHtml(data, file, configProvider.Config.LocationLink);
            var bboxesJs = HtmlExportUtils.BuildBboxesJs(file, data.Persons);
            var isPicture = file.FileType == FileType.Picture;
            string srcValue;
            if (isPicture)
            {
                var sourceFilePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
                var imageBytes = fileSystem.File.ReadAllBytes(sourceFilePath);
                var base64 = Convert.ToBase64String(imageBytes);
                var mime = GetImageMimeType(file.OriginalPath);
                srcValue = $"data:{mime};base64,{base64}";
            }
            else
            {
                srcValue = string.Empty;
            }
            var alt = Path.GetFileName(file.OriginalPath);
            slidesJs.AppendLine($"  {{ src: {ToJsString(srcValue)}, alt: {ToJsString(alt)}, meta: {ToJsString(metaHtml)}, isPicture: {(isPicture ? "true" : "false")}, bboxes: {bboxesJs} }}{(i < existingFiles.Count - 1 ? "," : "")}");
        }
        slidesJs.AppendLine("];");

        var html = BuildHtmlDocument(data, slidesJs.ToString());
        fileSystem.File.WriteAllText(destinationFilePath, html);
    }

    private static string ToJsString(string value) =>
        "`" + value.Replace("\\", "\\\\").Replace("`", "\\`").Replace("${", "\\${") + "`";

    private static string BuildHtmlDocument(RichExportData data, string slidesJs) =>
        HtmlExportUtils.BuildViewerHtml(data, slidesJs);

    private static string GetImageMimeType(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream",
        };
}


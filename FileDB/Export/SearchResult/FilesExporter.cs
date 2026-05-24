using FileDB.Model;
using FileDB.Model.FileFormats;
using System.IO;
using System.IO.Abstractions;

namespace FileDB.Export.SearchResult;

public class FilesExporter(IFileSystem fileSystem, IFilesystemAccessProvider filesystemAccessProvider) : ISearchResultExporter
{
    public void Export(SearchResultExport data, string path)
    {
        foreach (var file in data.Files)
        {
            var sourceFilePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
            var destFilePath = Path.Combine(path, file.ExportedPath);
            var destDir = Path.GetDirectoryName(destFilePath)!;
            if (!fileSystem.Directory.Exists(destDir))
            {
                fileSystem.Directory.CreateDirectory(destDir);
            }

            fileSystem.File.Copy(sourceFilePath, destFilePath);
        }
    }
}

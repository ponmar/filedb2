using FileDBAvalonia.Model;
using FileDBInterface.FileFormats;
using System.IO;
using System.IO.Abstractions;

namespace FileDBAvalonia.Export.SearchResult;

public class FilesExporter : ISearchResultExporter
{
    public void Export(SearchResultExport data, string path)
    {
        var fileSystemAccess = ServiceLocator.Resolve<IFilesystemAccessProvider>();
        var fileSystem = ServiceLocator.Resolve<IFileSystem>();

        foreach (var file in data.Files)
        {
            var sourceFilePath = fileSystemAccess.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
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

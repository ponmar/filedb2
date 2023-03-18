using FileDB.Model;
using FileDBShared.FileFormats;
using System.IO;

namespace FileDB.Export;

public class SearchResultFilesExporter : ISearchResultExporter
{
    public void Export(SearchResultExport data, string path)
    {
        var fileSyustemAccess = ServiceLocator.Resolve<IFilesystemAccessRepository>();
        foreach (var file in data.Files)
        {
            var sourceFilePath = fileSyustemAccess.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
            var destFilePath = Path.Combine(path, file.ExportedPath);
            var destDir = Path.GetDirectoryName(destFilePath)!;
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            File.Copy(sourceFilePath, destFilePath);
        }
    }
}

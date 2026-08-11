using FileDB.Model;
using FileDB.Model.FileFormats;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace FileDB.Export.SearchResult;

public interface IFilesExporter
{
    void Export(List<ExportedFile> files, string path);
}

public class FilesExporter(IFileSystem fileSystem, IFilesystemAccessProvider filesystemAccessProvider) : IFilesExporter
{
    public void Export(List<ExportedFile> files, string path)
    {
        foreach (var file in files)
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

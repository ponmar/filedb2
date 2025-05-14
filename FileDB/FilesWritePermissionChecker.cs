using FileDB.Model;

namespace FileDB;

public interface IFilesWritePermissionChecker
{
    bool HasWritePermission { get; }
}

public class FilesWritePermissionChecker : IFilesWritePermissionChecker
{
    public bool HasWritePermission { get; }

    public FilesWritePermissionChecker(IConfigProvider configProvider, IFilesystemAccessProvider filesystemAccessProvider)
    {
        try
        {
            var fs = filesystemAccessProvider.FilesystemAccess.FileSystem;
            var tempFilePath = fs.Path.Combine(configProvider.FilePaths.FilesRootDir, fs.Path.GetRandomFileName());
            using (var tempFile = fs.File.Create(tempFilePath)) { }
            fs.File.Delete(tempFilePath);
            HasWritePermission = true;
        }
        catch
        {
            HasWritePermission = false;
        }
    }
}

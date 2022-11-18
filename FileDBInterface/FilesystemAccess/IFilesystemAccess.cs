using System.Collections.Generic;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;

namespace FileDBInterface.FilesystemAccess
{
    public interface IFilesystemAccess
    {
        string FilesRootDirectory { get; set; }

        IEnumerable<string> ListNewFilesystemFiles(string path, IEnumerable<string> blacklistedFilePathPatterns, IEnumerable<string> whitelistedFilePathPatterns, bool includeHiddenDirectories, IFilesAccess filesDbAccess);
        bool PathIsApplicable(string internalPath, IEnumerable<string> blacklistedFilePathPatterns, IEnumerable<string> whitelistedFilePathPatterns, bool includeHiddenDirectories);
        IEnumerable<string> ListAllFilesystemDirectories();
        IEnumerable<FilesModel> GetFilesMissingInFilesystem(IEnumerable<FilesModel> allFiles);
        string ToAbsolutePath(string internalPath);
        FileMetadata ParseFileMetadata(string path);
    }
}

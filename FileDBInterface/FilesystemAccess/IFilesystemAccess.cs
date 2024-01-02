using System.Collections.Generic;
using System.IO.Abstractions;
using FileDBInterface.DbAccess;
using FileDBShared.Model;

namespace FileDBInterface.FilesystemAccess;

public interface IFilesystemAccess
{
    IFileSystem FileSystem { get; }
    IEnumerable<string> ListNewFilesystemFiles(string path, IEnumerable<string> blacklistedFilePathPatterns, IEnumerable<string> whitelistedFilePathPatterns, bool includeHiddenDirectories, IFilesAccess filesDbAccess);
    bool PathIsApplicable(string internalPath, IEnumerable<string> blacklistedFilePathPatterns, IEnumerable<string> whitelistedFilePathPatterns, bool includeHiddenDirectories);
    IEnumerable<string> ListAllFilesystemDirectories();
    IEnumerable<FileModel> GetFilesMissingInFilesystem(IEnumerable<FileModel> allFiles);
    string ToAbsolutePath(string internalPath);
    FileMetadata ParseFileMetadata(string path);
}

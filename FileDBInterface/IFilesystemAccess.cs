using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDBInterface
{
    public interface IFilesystemAccess
    {
        public IEnumerable<string> ListNewFilesystemFiles(IEnumerable<string> blacklistedFilePathPatterns, IEnumerable<string> whitelistedFilePathPatterns, bool includeHiddenDirectories);
        public IEnumerable<string> ListAllFilesystemDirectories();
        public IEnumerable<FilesModel> GetFilesMissingInFilesystem();
    }
}

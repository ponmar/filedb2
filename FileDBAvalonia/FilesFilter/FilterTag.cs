using FileDBAvalonia.ViewModels;
using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class FilterTag(TagForSearch? tag) : IFilesFilter
{
    public bool CanRun() => tag is not null;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesWithTags([tag!.Id]);
    }
}

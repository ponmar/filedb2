using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public interface IFilterViewModel
{
    IFilesFilter CreateFilter();
}

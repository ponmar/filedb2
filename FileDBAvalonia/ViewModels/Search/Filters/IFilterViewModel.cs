using FileDBAvalonia.FilesFilter;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public interface IFilterViewModel
{
    IFilesFilter CreateFilter();
}

using System.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public interface IFilterViewModel : INotifyDataErrorInfo
{
    IFilesFilter CreateFilter();
}

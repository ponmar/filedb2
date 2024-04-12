using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class FileListViewModel : AbstractFilterViewModel
{
    [ObservableProperty]
    private string fileListIds = string.Empty;

    [ObservableProperty]
    private bool negate;

    public FileListViewModel() : base(FilterType.FileList)
    {
    }

    protected override IFilesFilter DoCreate() =>
        Negate ? new FilterExceptFileList(FileListIds) : new FilterFileList(FileListIds);
}

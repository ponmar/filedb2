using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class DirectoryViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private string directoryPath = string.Empty;

    private readonly IDialogs dialogs;

    public DirectoryViewModel(IDialogs dialogs)
    {
        this.dialogs = dialogs;
        // TODO: select directory from current file?
    }

    [RelayCommand]
    private async Task FindBrowsedFilesAsync()
    {
        DirectoryPath = await dialogs.ShowBrowseDirectoriesDialogAsync() ?? string.Empty;
    }

    public IFilesFilter CreateFilter()
    {
        return new DirectoryFilter(DirectoryPath);
    }
}

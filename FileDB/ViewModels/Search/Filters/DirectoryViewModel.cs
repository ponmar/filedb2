using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.Lang;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class DirectoryViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    private string directoryPath = string.Empty;

    private readonly IConfigProvider configProvider;
    private readonly IDialogs dialogs;

    public DirectoryViewModel(IDialogs dialogs, IFileSelector fileSelector, IConfigProvider configProvider)
    {
        this.dialogs = dialogs;
        this.configProvider = configProvider;

        if (fileSelector.SelectedFile is not null)
        {
            var lastSlashIndex = fileSelector.SelectedFile.Path.LastIndexOf('/');
            if (lastSlashIndex != -1)
            {
                DirectoryPath = fileSelector.SelectedFile.Path[..lastSlashIndex];
            }
        }

        ValidateAllProperties();
    }

    [RelayCommand]
    private async Task FindBrowsedFilesAsync()
    {
        DirectoryPath = await dialogs.ShowBrowseExistingSubDirectoryDialogAsync(Strings.FilesSelectASubDirectory, configProvider.FilePaths.FilesRootDir) ?? string.Empty;
    }

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesByPath(DirectoryPath);
    }
}

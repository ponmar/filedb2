using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class FileListViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    private string fileListIds = string.Empty;

    [ObservableProperty]
    private bool negate;

    public bool HasSearchResult => searchResultRepository.Files.Any();

    private readonly ISearchResultRepository searchResultRepository;

    public FileListViewModel(ISearchResultRepository searchResultRepository)
    {
        this.searchResultRepository = searchResultRepository;

        this.RegisterForEvent<SearchResultRepositoryUpdated>(x => OnPropertyChanged(nameof(HasSearchResult)));

        ValidateAllProperties();
    }

    [RelayCommand]
    private void SetFromCurrent()
    {
        FileListIds = Utils.CreateFileList(searchResultRepository.Files);
    }

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        var fileIds = Utils.CreateFileIds(FileListIds);
        return dbAccess.SearchFilesExceptIds(fileIds);
    }
}

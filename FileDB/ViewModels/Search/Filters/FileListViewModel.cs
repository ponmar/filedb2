using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.FilesFilter;
using FileDB.Model;

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

    public IFilesFilter CreateFilter() =>
        Negate ? new ExceptFileListFilter(FileListIds) : new FileListFilter(FileListIds);
}

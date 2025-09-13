using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;
using FileDB.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class TextViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    private string textFilterSearchPattern = string.Empty;

    [ObservableProperty]
    private bool textFilterCaseSensitive = false;

    [ObservableProperty]
    private bool textFilterPersons;

    [ObservableProperty]
    private bool textFilterLocations;

    [ObservableProperty]
    private bool textFilterTags;

    private readonly IPersonsRepository personsRepository;
    private readonly ILocationsRepository locationsRepository;
    private readonly ITagsRepository tagsRepository;

    public TextViewModel(IPersonsRepository personsRepository, ILocationsRepository locationsRepository, ITagsRepository tagsRepository)
    {
        this.personsRepository = personsRepository;
        this.locationsRepository = locationsRepository;
        this.tagsRepository = tagsRepository;

        ValidateAllProperties();
    }

    public IFilesFilter CreateFilter() => new TextFilter(
        TextFilterSearchPattern, TextFilterCaseSensitive, TextFilterPersons, TextFilterLocations, TextFilterTags,
        personsRepository, locationsRepository, tagsRepository);
}

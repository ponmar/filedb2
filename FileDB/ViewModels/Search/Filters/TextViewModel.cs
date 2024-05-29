using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;
using FileDB.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class TextViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
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
    }

    public IFilesFilter CreateFilter() => new TextFilter(
        TextFilterSearchPattern, TextFilterCaseSensitive, TextFilterPersons, TextFilterLocations, TextFilterTags,
        personsRepository, locationsRepository, tagsRepository);
}

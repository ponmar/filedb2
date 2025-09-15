using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Comparers;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

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

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        var files = dbAccess.SearchFiles(TextFilterSearchPattern, TextFilterCaseSensitive).ToList();

        var stringComparison = TextFilterCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

        IEnumerable<FileModel> filesWithPersons = [];
        if (TextFilterPersons)
        {
            var machingPersons = personsRepository.Persons.Where(x => $"{x.Firstname} {x.Lastname}".Contains(TextFilterSearchPattern, stringComparison) || (x.Description is not null && x.Description.Contains(TextFilterSearchPattern, stringComparison))).Select(x => x.Id);
            filesWithPersons = dbAccess.SearchFilesWithPersons(machingPersons);
        }

        IEnumerable<FileModel> filesWithLocations = [];
        if (TextFilterLocations)
        {
            var machingLocations = locationsRepository.Locations.Where(x => x.Name.Contains(TextFilterSearchPattern, stringComparison) || (x.Description is not null && x.Description.Contains(TextFilterSearchPattern, stringComparison))).Select(x => x.Id);
            filesWithLocations = dbAccess.SearchFilesWithLocations(machingLocations);
        }

        IEnumerable<FileModel> filesWithTags = [];
        if (TextFilterTags)
        {
            var machingTags = tagsRepository.Tags.Where(x => x.Name.Contains(TextFilterSearchPattern, stringComparison)).Select(x => x.Id);
            filesWithTags = dbAccess.SearchFilesWithTags(machingTags);
        }

        var fileModelComparer = new FileModelByIdComparer();

        return files
            .Union(filesWithPersons, fileModelComparer)
            .Union(filesWithLocations, fileModelComparer)
            .Union(filesWithTags, fileModelComparer);
    }
}

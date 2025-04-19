using System;
using FileDB.Lang;
using FileDB.ViewModels.Search.Filters;

namespace FileDB.Model;

public class FilterTypeViewModelAttribute(Type viewModelType) : Attribute
{
    public Type ViewModelType { get; } = viewModelType;
}

public enum FilterType
{
    [FilterTypeViewModel(typeof(TextViewModel))]
    Text,

    [FilterTypeViewModel(typeof(DateViewModel))]
    Date,

    [FilterTypeViewModel(typeof(NoMetaDataViewModel))]
    NoMetaData,

    [FilterTypeViewModel(typeof(NoDateTimeViewModel))]
    NoDateTime,

    [FilterTypeViewModel(typeof(FileListViewModel))]
    FileList,

    [FilterTypeViewModel(typeof(FileTypeViewModel))]
    FileType,

    [FilterTypeViewModel(typeof(PersonViewModel))]
    Person,

    [FilterTypeViewModel(typeof(PersonAgeViewModel))]
    PersonAge,

    [FilterTypeViewModel(typeof(PersonSexViewModel))]
    PersonSex,

    [FilterTypeViewModel(typeof(PersonGroupViewModel))]
    PersonGroup,

    [FilterTypeViewModel(typeof(LocationViewModel))]
    Location,

    [FilterTypeViewModel(typeof(TagViewModel))]
    Tag,

    [FilterTypeViewModel(typeof(TagsViewModel))]
    Tags,

    [FilterTypeViewModel(typeof(PositionViewModel))]
    Position,

    [FilterTypeViewModel(typeof(NumPersonsViewModel))]
    NumPersons,

    [FilterTypeViewModel(typeof(SeasonViewModel))]
    Season,

    [FilterTypeViewModel(typeof(AnnualDateViewModel))]
    AnnualDate,

    [FilterTypeViewModel(typeof(TimeViewModel))]
    Time,
    
    [FilterTypeViewModel(typeof(AllFilesViewModel))]
    AllFiles,

    [FilterTypeViewModel(typeof(DirectoryViewModel))]
    Directory,

    [FilterTypeViewModel(typeof(RandomViewModel))]
    Random,

    [FilterTypeViewModel(typeof(CombineViewModel))]
    Combine,
}

public static class FilterTypeExtensions
{
    public static string ToFriendlyString(this FilterType filterType)
    {
        return filterType switch
        {
            FilterType.Date => Strings.FilterTypeDateTime,
            FilterType.NoMetaData => Strings.FilterTypeNoMetaData,
            FilterType.NoDateTime => Strings.FilterTypeNoDateTime,
            FilterType.Text => Strings.FilterTypeText,
            FilterType.FileList => Strings.FilterTypeFileList,
            FilterType.FileType => Strings.FilterTypeFileType,
            FilterType.Person => Strings.FilterTypePerson,
            FilterType.PersonAge => Strings.FilterTypePersonAge,
            FilterType.PersonSex => Strings.FilterTypePersonSex,
            FilterType.PersonGroup => Strings.FilterTypePersonGroup,
            FilterType.Location => Strings.FilterTypeLocation,
            FilterType.Tag => Strings.FilterTypeTag,
            FilterType.Tags => Strings.FilterTypeTags,
            FilterType.Position => Strings.FilterTypePosition,
            FilterType.NumPersons => Strings.FilterTypeNumPersons,
            FilterType.Season => Strings.FilterTypeSeason,
            FilterType.AnnualDate => Strings.FilterTypeAnnualDate,
            FilterType.Time => Strings.FilterTypeTime,
            FilterType.AllFiles => Strings.FilterTypeAllFiles,
            FilterType.Directory => Strings.FilterTypeDirectory,
            FilterType.Random => Strings.FilterTypeRandom,
            FilterType.Combine => Strings.FilterTypeCombine,
            _ => throw new NotImplementedException(),
        };
    }
}
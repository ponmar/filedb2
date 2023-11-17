using FileDB.Resources;
using System;

namespace FileDB.Model;

public enum FilterType
{
    NoMetaData,
    NoDateTime,
    Text,
    FileList,
    ExceptFileList,
    FileType,
    Person,
    PersonAge,
    Location,
    Tag,
}

public static class FilterTypeExtensions
{
    public static string ToFriendlyString(this FilterType filterType)
    {
        return filterType switch
        {
            FilterType.NoMetaData => Strings.FilterTypeNoMetaData,
            FilterType.NoDateTime => Strings.FilterTypeNoDateTime,
            FilterType.Text => Strings.FilterTypeText,
            FilterType.FileList => Strings.FilterTypeFileList,
            FilterType.ExceptFileList => Strings.FilterTypeExceptFileList,
            FilterType.FileType => Strings.FilterTypeFileType,
            FilterType.Person => Strings.FilterTypePerson,
            FilterType.PersonAge => Strings.FilterTypePersonAge,
            FilterType.Location => Strings.FilterTypeLocation,
            FilterType.Tag => Strings.FilterTypeTag,
            _ => throw new NotImplementedException(),
        };
    }
}
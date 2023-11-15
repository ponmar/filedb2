using FileDB.Resources;
using System;

namespace FileDB.Model;

public enum FilterType
{
    NoMetaData,
    NoDateTime,
    Text,
    FileList,
    FileType,
    Person,
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
            FilterType.FileType => Strings.FilterTypeFileType,
            FilterType.Person => Strings.FilterTypePerson,
            _ => throw new NotImplementedException(),
        };
    }
}
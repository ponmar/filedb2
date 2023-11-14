using FileDB.Resources;
using System;

namespace FileDB.Model;

public enum FilterType
{
    NoMetaData,
    NoDateTime,
    Text,
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
            _ => throw new NotImplementedException(),
        };
    }
}
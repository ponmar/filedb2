﻿using System;
using FileDBAvalonia.Lang;

namespace FileDBAvalonia.Model;

public enum FilterType
{
    DateTime,
    NoMetaData,
    NoDateTime,
    Text,
    FileList,
    ExceptFileList,
    FileType,
    Person,
    PersonAge,
    PersonSex,
    Location,
    Tag,
    Position,
    NumPersons,
}

public static class FilterTypeExtensions
{
    public static string ToFriendlyString(this FilterType filterType)
    {
        return filterType switch
        {
            FilterType.DateTime => Strings.FilterTypeDateTime,
            FilterType.NoMetaData => Strings.FilterTypeNoMetaData,
            FilterType.NoDateTime => Strings.FilterTypeNoDateTime,
            FilterType.Text => Strings.FilterTypeText,
            FilterType.FileList => Strings.FilterTypeFileList,
            FilterType.ExceptFileList => Strings.FilterTypeExceptFileList,
            FilterType.FileType => Strings.FilterTypeFileType,
            FilterType.Person => Strings.FilterTypePerson,
            FilterType.PersonAge => Strings.FilterTypePersonAge,
            FilterType.PersonSex => Strings.FilterTypePersonSex,
            FilterType.Location => Strings.FilterTypeLocation,
            FilterType.Tag => Strings.FilterTypeTag,
            FilterType.Position => Strings.FilterTypePosition,
            FilterType.NumPersons => Strings.FilterTypeNumPersons,
            _ => throw new NotImplementedException(),
        };
    }
}
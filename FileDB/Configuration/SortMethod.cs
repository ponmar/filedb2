using FileDB.Lang;
using System;

namespace FileDB.Configuration;

public enum SortMethod
{
    Date,
    DateDesc,
    Path,
    PathDesc,
    Random,
    NumPersons,
    NumPersonsDesc,
    DirectoryDate,
    DirectoryDateDesc,
}

public static class SortMethodExtensions
{
    public static string ToFriendlyString(this SortMethod sortMethod)
    {
        return sortMethod switch
        {
            SortMethod.Date => Strings.SortMethodDate,
            SortMethod.DateDesc => Strings.SortMethodDateDesc,
            SortMethod.Path => Strings.SortMethodPath,
            SortMethod.PathDesc => Strings.SortMethodPathDesc,
            SortMethod.Random => Strings.SortMethodRandom,
            SortMethod.NumPersons => Strings.SortMethodNumPersons,
            SortMethod.NumPersonsDesc => Strings.SortMethodNumPersonsDesc,
            SortMethod.DirectoryDate => Strings.SortMethodDirectoryDate,
            SortMethod.DirectoryDateDesc => Strings.SortMethodDirectoryDateDesc,
            _ => throw new NotSupportedException(),
        };
    }
}

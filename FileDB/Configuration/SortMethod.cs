using FileDB.Lang;
using System;

namespace FileDB.Configuration;

public enum SortMethod { Date, DateDesc, Path, PathDesc }

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
            _ => throw new NotSupportedException(),
        };
    }
}

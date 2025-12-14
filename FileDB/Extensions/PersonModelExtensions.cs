using System;
using FileDBInterface.Extensions;
using FileDBInterface.Model;

namespace FileDB.Extensions;

public static class PersonModelExtensions
{
    private const StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;

    public static bool MatchesTextFilter(this PersonModel personModel, string textFilter)
    {
        return !textFilter.HasContent() ||
            personModel.ShortName.Contains(textFilter, stringComparison) ||
            personModel.FullName.Contains(textFilter, stringComparison) ||
            (personModel.Description is not null && personModel.Description.Contains(textFilter, stringComparison));
    }
}

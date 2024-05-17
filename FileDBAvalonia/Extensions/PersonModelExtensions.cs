using System;
using FileDBInterface.Extensions;
using FileDBInterface.Model;

namespace FileDBAvalonia.Extensions;

public static class PersonModelExtensions
{
    private const StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;

    public static bool MatchesTextFilter(this PersonModel personModel, string textFilter)
    {
        return !textFilter.HasContent() ||
            personModel.Firstname.Contains(textFilter, stringComparison) ||
            personModel.Lastname.Contains(textFilter, stringComparison) ||
            $"{personModel.Firstname} {personModel.Lastname}".Contains(textFilter, stringComparison) ||
            (personModel.Description is not null && personModel.Description.Contains(textFilter, stringComparison));
    }
}

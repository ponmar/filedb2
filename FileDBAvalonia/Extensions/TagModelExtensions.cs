using System;
using FileDBShared.Model;

namespace FileDBAvalonia.Extensions;

public static class TagModelExtensions
{
    private const StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;

    public static bool MatchesTextFilter(this TagModel tagModel, string textFilter)
    {
        return tagModel.Name.Contains(textFilter, stringComparison);
    }
}

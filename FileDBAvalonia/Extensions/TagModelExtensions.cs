using System;
using FileDBInterface.Extensions;
using FileDBInterface.Model;

namespace FileDBAvalonia.Extensions;

public static class TagModelExtensions
{
    private const StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;

    public static bool MatchesTextFilter(this TagModel tagModel, string textFilter)
    {
        return !textFilter.HasContent() || 
            tagModel.Name.Contains(textFilter, stringComparison);
    }
}

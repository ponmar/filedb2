using System;
using FileDBInterface.Extensions;
using FileDBInterface.Model;

namespace FileDBAvalonia.Extensions;

public static class LocationModelExtensions
{
    private const StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;

    public static bool MatchesTextFilter(this LocationModel tagModel, string textFilter)
    {
        return !textFilter.HasContent() ||
            tagModel.Name.Contains(textFilter, stringComparison) ||
            (tagModel.Description is not null && tagModel.Description.Contains(textFilter, stringComparison));
    }
}

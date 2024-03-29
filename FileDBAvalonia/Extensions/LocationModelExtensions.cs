using FileDBShared.Model;

namespace FileDBAvalonia.Extensions;

public static class LocationModelExtensions
{
    public static bool MatchesTextFilter(this LocationModel tagModel, string textFilter)
    {
        return tagModel.Name.Contains(textFilter) ||
            (tagModel.Description is not null && tagModel.Description.Contains(textFilter));
    }
}

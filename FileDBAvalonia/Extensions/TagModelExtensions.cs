using FileDBShared.Model;

namespace FileDBAvalonia.Extensions;

public static class TagModelExtensions
{
    public static bool MatchesTextFilter(this TagModel tagModel, string textFilter)
    {
        return tagModel.Name.Contains(textFilter);
    }
}

using FileDBShared.Model;

namespace FileDBAvalonia.Extensions;

public static class PersonModelExtensions
{
    public static bool MatchesTextFilter(this PersonModel personModel, string textFilter)
    {
        return personModel.Firstname.Contains(textFilter) ||
            personModel.Lastname.Contains(textFilter) ||
            $"{personModel.Firstname} {personModel.Lastname}".Contains(textFilter) ||
            (personModel.Description is not null && personModel.Description.Contains(textFilter));
    }
}

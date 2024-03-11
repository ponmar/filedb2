using FileDBAvalonia.Lang;
using FileDBShared.Model;
using System;

namespace FileDBAvalonia.Extensions;

public static class SexExtensions
{
    public static string ToFriendlyString(this Sex sex)
    {
        return sex switch
        {
            Sex.NotKnown => Strings.SexNotKnown,
            Sex.Male => Strings.SexMale,
            Sex.Female => Strings.SexFemale,
            Sex.NotApplicable => Strings.SexNotApplicable,
            _ => throw new NotSupportedException(),
        };
    }
}

using FileDB.Resources;
using FileDBShared.Model;
using System;

namespace FileDB.Extensions;

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

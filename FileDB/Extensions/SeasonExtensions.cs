using FileDB.Lang;
using FileDBInterface.DatabaseAccess;
using System;

namespace FileDB.Extensions;

public static class SeasonExtensions
{
    public static string ToFriendlyString(this Season season)
    {
        return season switch
        {
            Season.Spring => Strings.SeasonSpring,
            Season.Summer => Strings.SeasonSummer,
            Season.Autumn => Strings.SeasonAutumn,
            Season.Winter => Strings.SeasonWinter,
            _ => throw new NotSupportedException(),
        };
    }
}

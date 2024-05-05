using FileDBAvalonia.Configuration;
using FileDBAvalonia.Lang;
using System;

namespace FileDBAvalonia.Extensions;

public static class ThemeExtensions
{
    public static string ToFriendlyString(this Theme theme)
    {
        return theme switch
        {
            Theme.Default => Strings.ThemeDefault,
            Theme.Dark => Strings.ThemeDark,
            Theme.Light => Strings.ThemeLight,
            _ => throw new NotSupportedException(),
        };
    }
}

﻿using FileDB.Configuration;
using FileDB.Lang;
using System;

namespace FileDB.Extensions;

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

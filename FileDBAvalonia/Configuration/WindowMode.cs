using FileDBAvalonia.Lang;
using System;

namespace FileDBAvalonia.Configuration;

public enum WindowMode { Normal, Maximized, Fullscreen }

public static class WindowModeExtensions
{
    public static string ToFriendlyString(this WindowMode windowMode)
    {
        return windowMode switch
        {
            WindowMode.Normal => Strings.WindowModeNormal,
            WindowMode.Maximized => Strings.WindowModeMaximized,
            WindowMode.Fullscreen => Strings.WindowModeFullscreen,
            _ => throw new NotImplementedException(),
        };
    }
}

using Avalonia.Controls;
using FileDB.Lang;
using System;

namespace FileDB.Configuration;

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

    public static WindowState ToWindowState(this WindowMode windowMode)
    {
        return windowMode switch
        {
            WindowMode.Normal => WindowState.Normal,
            WindowMode.Maximized => WindowState.Maximized,
            WindowMode.Fullscreen => WindowState.FullScreen,
            _ => throw new NotImplementedException(),
        };
    }
}

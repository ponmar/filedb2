using FileDB.Configuration;

namespace FileDB.Extensions;

public static class WindowModeExtensions
{
    public static string GetDescription(this WindowMode mode)
    {
        switch (mode)
        {
            case WindowMode.Normal:
                return "Window";

            case WindowMode.Maximized:
                return "Maximized window";

            case WindowMode.Fullscreen:
                return "Fullscreen window";
        }
        return string.Empty;
    }
}

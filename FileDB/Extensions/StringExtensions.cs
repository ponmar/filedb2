namespace FileDB.Extensions;

public static class StringExtensions
{
    public static bool HasContent(this string? str)
    {
        return !string.IsNullOrEmpty(str);
    }
}

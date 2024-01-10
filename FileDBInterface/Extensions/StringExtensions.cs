namespace FileDBInterface.Extensions;

public static class StringExtensions
{
    public static bool HasContent(this string? str)
    {
        return !string.IsNullOrEmpty(str);
    }

    public static string TextBeforeLast(this string str, string search)
    {
        return str[..str.LastIndexOf(search)];
    }
}

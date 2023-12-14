namespace FileDBInterface.Extensions;

public static class StringExtensions
{
    public static string TextBeforeLast(this string str, string search)
    {
        return str[..str.LastIndexOf(search)];
    }
}

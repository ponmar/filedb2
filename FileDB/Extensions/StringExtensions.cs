using Newtonsoft.Json;
using System.IO.Abstractions;

namespace FileDB.Extensions;

public static class StringExtensions
{
    public static bool HasContent(this string? str)
    {
        return !string.IsNullOrEmpty(str);
    }

    public static T FromJson<T>(this string filePath, IFileSystem fileSystem)
    {
        var json = fileSystem.File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<T>(json)!;
    }
}

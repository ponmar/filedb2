using Newtonsoft.Json;
using System.IO.Abstractions;

namespace FileDB.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Returns null for an empty file
    /// </summary>
    public static T? FromJson<T>(this string filePath, IFileSystem fileSystem)
    {
        var json = fileSystem.File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<T>(json);
    }
}

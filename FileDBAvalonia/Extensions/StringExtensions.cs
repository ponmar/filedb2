using Newtonsoft.Json;
using System.IO.Abstractions;

namespace FileDBAvalonia.Extensions;

public static class StringExtensions
{
    public static T FromJson<T>(this string filePath, IFileSystem fileSystem)
    {
        var json = fileSystem.File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<T>(json)!;
    }
}

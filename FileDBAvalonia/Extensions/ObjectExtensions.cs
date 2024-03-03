using Newtonsoft.Json;

namespace FileDBAvalonia.Extensions;

public static class ObjectExtensions
{
    public static string ToJson(this object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static string ToFormattedJson(this object obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }
}

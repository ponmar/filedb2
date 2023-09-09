using FileDB.Configuration;

namespace FileDB.Sorters;

public class SortMethodDescription
{
    public string Name => Method.ToFriendlyString();

    public SortMethod Method { get; }

    public SortMethodDescription(SortMethod method)
    {
        Method = method;
    }
}

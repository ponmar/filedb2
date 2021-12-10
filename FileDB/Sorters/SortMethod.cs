using FileDB.Config;
using FileDB.Extensions;

namespace FileDB.Sorters
{
    public class SortMethodDescription
    {
        public string Name => Method.GetDescription();

        public SortMethod Method { get; }

        public SortMethodDescription(SortMethod method)
        {
            Method = method;
        }
    }
}

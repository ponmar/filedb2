using Newtonsoft.Json;
using System.IO;

namespace FileDB.Export
{
    public class SearchResultJsonExporter : ISearchResultExporter
    {
        public void Export(SearchResultFileFormat data, string filename)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}

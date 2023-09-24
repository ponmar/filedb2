using FileDB.Extensions;
using FileDBShared.FileFormats;
using System.IO;

namespace FileDB.Export;

public class SearchResultJsonExporter : ISearchResultExporter
{
    public void Export(SearchResultExport data, string filename)
    {
        var json = data.ToFormattedJson();
        File.WriteAllText(filename, json);
    }
}

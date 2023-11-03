using FileDB.Extensions;
using FileDBShared.FileFormats;
using System.IO.Abstractions;

namespace FileDB.Export.SearchResult;

public class JsonExporter : ISearchResultExporter
{
    private readonly IFileSystem fileSystem;

    public JsonExporter(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    public void Export(SearchResultExport data, string filename)
    {
        var json = data.ToFormattedJson();
        fileSystem.File.WriteAllText(filename, json);
    }
}

using FileDB.Extensions;
using FileDBShared.FileFormats;
using System.IO.Abstractions;

namespace FileDB.Export;

public class SearchResultJsonExporter : ISearchResultExporter
{
    private readonly IFileSystem fileSystem;

    public SearchResultJsonExporter(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    public void Export(SearchResultExport data, string filename)
    {
        var json = data.ToFormattedJson();
        fileSystem.File.WriteAllText(filename, json);
    }
}

using FileDB.Extensions;
using FileDB.Model.FileFormats;
using System.IO.Abstractions;

namespace FileDB.Export.SearchResult;

public interface IJsonExporter
{
    void Export(JsonExportData data, string filename);
}

public class JsonExporter(IFileSystem fileSystem) : IJsonExporter
{
    public void Export(JsonExportData data, string filename)
    {
        var json = data.ToFormattedJson();
        fileSystem.File.WriteAllText(filename, json);
    }
}

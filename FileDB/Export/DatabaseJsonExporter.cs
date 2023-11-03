using FileDB.Extensions;
using FileDBShared.FileFormats;
using System.IO.Abstractions;

namespace FileDB.Export;

public class DatabaseJsonExporter
{
    private readonly IFileSystem fileSystem;

    public DatabaseJsonExporter(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    public void Export(DatabaseExport data, string filename)
    {
        var json = data.ToFormattedJson();
        fileSystem.File.WriteAllText(filename, json);
    }
}

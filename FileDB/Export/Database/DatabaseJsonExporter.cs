using FileDB.Extensions;
using FileDBInterface.FileFormats;
using System.IO.Abstractions;

namespace FileDB.Export.Database;

public class DatabaseJsonExporter : IDatabaseExporter
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

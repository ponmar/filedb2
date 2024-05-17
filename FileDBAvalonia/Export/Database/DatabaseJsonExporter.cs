using FileDBAvalonia.Extensions;
using FileDBInterface.FileFormats;
using System.IO.Abstractions;

namespace FileDBAvalonia.Export.Database;

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

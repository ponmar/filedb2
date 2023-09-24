using FileDB.Extensions;
using FileDBShared.FileFormats;
using System.IO;

namespace FileDB.Export;

public class DatabaseJsonExporter
{
    public void Export(DatabaseExport data, string filename)
    {
        var json = data.ToFormattedJson();
        File.WriteAllText(filename, json);
    }
}

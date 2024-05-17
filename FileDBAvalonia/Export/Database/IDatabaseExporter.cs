using FileDBInterface.FileFormats;

namespace FileDBAvalonia.Export.Database;

public interface IDatabaseExporter
{
    void Export(DatabaseExport data, string filename);
}

using FileDBInterface.FileFormats;

namespace FileDB.Export.Database;

public interface IDatabaseExporter
{
    void Export(DatabaseExport data, string filename);
}

using FileDBShared.FileFormats;

namespace FileDB.Export;

public interface ISearchResultExporter
{
    void Export(SearchResultExport data, string path);
}

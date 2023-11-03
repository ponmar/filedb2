using FileDBShared.FileFormats;

namespace FileDB.Export.SearchResult;

public interface ISearchResultExporter
{
    void Export(SearchResultExport data, string path);
}

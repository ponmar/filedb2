using FileDBInterface.FileFormats;

namespace FileDBAvalonia.Export.SearchResult;

public interface ISearchResultExporter
{
    void Export(SearchResultExport data, string path);
}

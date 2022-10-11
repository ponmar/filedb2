namespace FileDB.Export;

public interface ISearchResultExporter
{
    void Export(SearchResultFileFormat data, string path);
}

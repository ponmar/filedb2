using FileDB.ViewModels.Search;

namespace FileDB.ViewModels.Dialogs;

public class ExportSearchResultViewModel(
    FilesExportTabViewModel filesTab,
    M3uExportTabViewModel m3uTab,
    HtmlExportTabViewModel htmlTab,
    SelfContainedHtmlExportTabViewModel selfContainedHtmlTab,
    JsonExportTabViewModel jsonTab,
    PdfExportTabViewModel pdfTab)
{
    public FilesExportTabViewModel FilesTab { get; } = filesTab;
    public M3uExportTabViewModel M3uTab { get; } = m3uTab;
    public HtmlExportTabViewModel HtmlTab { get; } = htmlTab;
    public SelfContainedHtmlExportTabViewModel SelfContainedHtmlTab { get; } = selfContainedHtmlTab;
    public JsonExportTabViewModel JsonTab { get; } = jsonTab;
    public PdfExportTabViewModel PdfTab { get; } = pdfTab;

    public SearchResult? SearchResult
    {
        set
        {
            FilesTab.SearchResult = value;
            M3uTab.SearchResult = value;
            HtmlTab.SearchResult = value;
            SelfContainedHtmlTab.SearchResult = value;
            JsonTab.SearchResult = value;
            PdfTab.SearchResult = value;
        }
    }
}

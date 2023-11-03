using FileDBShared.FileFormats;
using QuestPDF.Fluent;
using System.IO.Abstractions;

namespace FileDB.Export;

public class SearchResultPdfExporter : ISearchResultExporter
{
    private readonly IFileSystem fileSystem;

    public SearchResultPdfExporter(IFileSystem fileSystem)
    { 
        this.fileSystem = fileSystem;
    }

    public void Export(SearchResultExport data, string filename)
    {
        var document = Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Header().Text("FileDB");
            });
        });

        var pdf = document.GeneratePdf();
        fileSystem.File.WriteAllBytes(filename, pdf);
    }
}

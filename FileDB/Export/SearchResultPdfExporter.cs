using FileDB.Model;
using FileDBShared.FileFormats;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.IO.Abstractions;

namespace FileDB.Export;

public class SearchResultPdfExporter : ISearchResultExporter
{
    private readonly IFileSystem fileSystem;
    private readonly IFilesystemAccessRepository filesystemAccessRepository;
    private readonly PageSize pageSize;

    public SearchResultPdfExporter(IFileSystem fileSystem, IFilesystemAccessRepository filesystemAccessRepository, PageSize pageSize)
    { 
        this.fileSystem = fileSystem;
        this.filesystemAccessRepository = filesystemAccessRepository;
        this.pageSize = pageSize;
    }

    public void Export(SearchResultExport data, string filename)
    {
        var document = Document.Create(document =>
        {
            document.Page(frontPage =>
            {
                frontPage.Size(pageSize);
                frontPage.Content().AlignCenter().AlignMiddle().Text(data.Header).SemiBold().FontSize(32).FontColor(Colors.Blue.Darken2);
                frontPage.Footer().AlignCenter().Text(data.About);
            });

            // TODO: only include images
            foreach (var file in data.Files)
            {
                document.Page(filePage =>
                {
                    filePage.Size(pageSize);
                    filePage.Content().AlignCenter().AlignMiddle().Column(column =>
                    {
                        var sourceFilePath = filesystemAccessRepository.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
                        column.Item().Image(sourceFilePath);
                    });
                });
            }
        });

        var pdf = document.GeneratePdf();
        fileSystem.File.WriteAllBytes(filename, pdf);
    }
}

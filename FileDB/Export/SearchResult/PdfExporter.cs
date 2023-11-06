using FileDB.Extensions;
using FileDB.Model;
using FileDB.ViewModel;
using FileDBShared.FileFormats;
using FileDBShared.Model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.IO.Abstractions;
using System.Linq;

namespace FileDB.Export.SearchResult;

public class PdfExporter : ISearchResultExporter
{
    private readonly IFileSystem fileSystem;
    private readonly IFilesystemAccessRepository filesystemAccessRepository;
    private readonly PageSize pageSize;

    public PdfExporter(IFileSystem fileSystem, IFilesystemAccessRepository filesystemAccessRepository, PageSize pageSize)
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
                frontPage.Margin(10);
                frontPage.Size(pageSize);
                frontPage.Content().AlignCenter().AlignMiddle().Text(data.Name).SemiBold().FontSize(32).FontColor(Colors.Blue.Darken2);
                frontPage.Footer().AlignCenter().AlignMiddle().Text(text =>
                {
                    text.Span($"{Utils.ApplicationName} {data.FileDBVersion} {data.ExportDateTime:yyyy-MM-dd HH:mm} ");
                    text.Hyperlink(data.ApplicationProjectUrl, data.ApplicationProjectUrl);
                });
            });

            var index = 1;
            foreach (var file in data.Files.Where(x => FileTypeUtils.GetFileType(x.OriginalPath) == FileType.Picture))
            {
                var pictureDateText = string.Empty;
                if (file.Datetime != null)
                {
                    pictureDateText = HtmlExporter.CreateExportedFileDatetime(file.Datetime);
                }

                var pictureDescription = string.Empty;
                if (file.Description != null)
                {
                    if (pictureDateText.HasContent())
                    {
                        pictureDescription += ": ";
                    }
                    pictureDescription += file.Description;
                }

                var fileHeading = $"[{index} / {data.Files.Count}] {pictureDateText}{pictureDescription}";

                string? filePersons = null;
                if (file.PersonIds.Count > 0)
                {
                    var persons = file.PersonIds.Select(x => data.Persons.First(y => y.Id == x));
                    var personStrings = persons.Select(x => $"{x.Firstname} {x.Lastname}{Utils.GetPersonAgeInFileString(file.Datetime, x.DateOfBirth)}").ToList();
                    personStrings.Sort();
                    filePersons = string.Join(", ", personStrings);
                }

                string? fileLocations = null;
                if (file.LocationIds.Count > 0)
                {
                    var locations = file.LocationIds.Select(x => data.Locations.First(y => y.Id == x));
                    var locationStrings = locations.Select(x => x.Name).ToList();
                    locationStrings.Sort();
                    fileLocations = string.Join(", ", locationStrings);
                }

                string? fileTags = null;
                if (file.TagIds.Count > 0)
                {
                    var tags = file.TagIds.Select(x => data.Tags.First(y => y.Id == x));
                    var tagStrings = tags.Select(x => x.Name).ToList();
                    tagStrings.Sort();
                    fileTags = string.Join(", ", tagStrings);
                }

                document.Page(filePage =>
                {
                    filePage.Margin(10);
                    filePage.Size(pageSize);
                    filePage.Header().AlignCenter().Text(fileHeading).SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);
                    filePage.Content().AlignCenter().AlignMiddle().Column(column =>
                    {
                        if (filePersons is not null)
                        {
                            column.Item().Text(filePersons);
                        }
                        if (fileLocations is not null)
                        {
                            column.Item().Text(fileLocations);
                        }
                        if (fileTags is not null)
                        {
                            column.Item().Text(fileTags);
                        }

                        var item = column.Item();
                        var degrees = DatabaseParsing.OrientationToDegrees(file.Orientation);
                        for (int i = 0; i < degrees / 90; i++)
                        {
                            item = item.RotateLeft();
                        }
                        var sourceFilePath = filesystemAccessRepository.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
                        item.Image(sourceFilePath);
                    });
                });

                index++;
            }
        });

        var pdf = document.GeneratePdf();
        fileSystem.File.WriteAllBytes(filename, pdf);
    }
}

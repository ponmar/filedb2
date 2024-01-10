using FileDB.Extensions;
using FileDB.Model;
using FileDB.ViewModel;
using FileDBInterface.Extensions;
using FileDBShared.FileFormats;
using FileDBShared.Model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.IO.Abstractions;
using System.Linq;

namespace FileDB.Export.SearchResult;

public class PdfExporter(IFileSystem fileSystem, IFilesystemAccessProvider filesystemAccessProvider, PageSize pageSize) : ISearchResultExporter
{
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
                    text.Span($"{Utils.ApplicationName} {data.FileDBVersion} {data.ExportDateTime.ToDateAndTime()} ");
                    text.Hyperlink(data.ApplicationProjectUrl, data.ApplicationProjectUrl);
                });
            });

            foreach (var picture in data.Files.Where(x => x.FileType == FileType.Picture))
            {
                var pictureDateText = string.Empty;
                if (picture.Datetime is not null)
                {
                    pictureDateText = HtmlExporter.CreateExportedFileDatetime(picture.Datetime);
                }

                var pictureDescription = string.Empty;
                if (picture.Description is not null)
                {
                    if (pictureDateText.HasContent())
                    {
                        pictureDescription += ": ";
                    }
                    pictureDescription += picture.Description;
                }

                var fileHeading = $"{pictureDateText}{pictureDescription}";

                string? filePersons = null;
                if (picture.PersonIds.Count > 0)
                {
                    var persons = data.Persons.Where(x => picture.PersonIds.Contains(x.Id));
                    filePersons = FileTextOverlayCreator.GetPersonsText(picture.Datetime, persons, ", ");
                }

                string? fileLocations = null;
                if (picture.LocationIds.Count > 0)
                {
                    var locations = data.Locations.Where(x => picture.LocationIds.Contains(x.Id));
                    fileLocations = FileTextOverlayCreator.GetLocationsText(locations, ", ");
                }

                string? fileTags = null;
                if (picture.TagIds.Count > 0)
                {
                    var tags = data.Tags.Where(x => picture.TagIds.Contains(x.Id));
                    fileTags = FileTextOverlayCreator.GetTagsText(tags, ", ");
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
                        var degrees = DatabaseParsing.OrientationToDegrees(picture.Orientation);
                        for (int i = 0; i < degrees / 90; i++)
                        {
                            item = item.RotateLeft();
                        }
                        var sourceFilePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(picture.OriginalPath);
                        item.Image(sourceFilePath);
                    });
                    filePage.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            }
        });

        var pdf = document.GeneratePdf();
        fileSystem.File.WriteAllBytes(filename, pdf);
    }
}

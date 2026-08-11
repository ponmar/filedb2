using FileDB.Extensions;
using FileDB.Model;
using FileDB.Model.FileFormats;
using FileDBInterface.Extensions;
using FileDBInterface.Model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace FileDB.Export.SearchResult;

public interface IPdfExporter
{
    void Export(RichExportData data, string filename);
}

public class PdfExporter(IFileSystem fileSystem, IFilesystemAccessProvider filesystemAccessProvider, IConfigProvider configProvider) : IPdfExporter
{
    private static readonly PageSize PageSize = PageSizes.A4.Landscape();

    public void Export(RichExportData data, string filename)
    {
        var document = Document.Create(document =>
        {
            document.Page(frontPage =>
            {
                frontPage.Margin(10);
                frontPage.Size(PageSize);
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
                var fileLocationItems = new List<(string Name, string? Link)>();
                if (picture.LocationIds.Count > 0)
                {
                    var locations = data.Locations.Where(x => picture.LocationIds.Contains(x.Id));
                    fileLocations = FileTextOverlayCreator.GetLocationsText(locations, ", ");
                    fileLocationItems = locations
                        .Select(l => (l.Name, Utils.CreatePositionLink(l.Position, configProvider.Config.LocationLink)))
                        .ToList();
                }

                string? fileTags = null;
                if (picture.TagIds.Count > 0)
                {
                    var tags = data.Tags.Where(x => picture.TagIds.Contains(x.Id));
                    fileTags = FileTextOverlayCreator.GetTagsText(tags, ", ");
                }

                var filePositionLink = Utils.CreatePositionLink(picture.Position, configProvider.Config.LocationLink);

                document.Page(filePage =>
                {
                    filePage.Margin(10);
                    filePage.Size(PageSize);
                    filePage.Header().Column(headerCol =>
                    {
                        headerCol.Item().AlignCenter().Text(fileHeading).SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);
                        if (filePersons is not null)
                        {
                            headerCol.Item().AlignCenter().Text(filePersons);
                        }
                        if (fileLocations is not null)
                        {
                            headerCol.Item().AlignCenter().Text(text =>
                            {
                                bool first = true;
                                foreach (var (name, link) in fileLocationItems)
                                {
                                    if (!first) text.Span(", ");
                                    if (link is not null)
                                        text.Hyperlink(name, link);
                                    else
                                        text.Span(name);
                                    first = false;
                                }
                            });
                        }
                        if (fileTags is not null)
                        {
                            headerCol.Item().AlignCenter().Text(fileTags);
                        }
                        if (filePositionLink is not null)
                        {
                            headerCol.Item().AlignCenter().Text(text => text.Hyperlink(picture.Position!, filePositionLink));
                        }
                        else if (picture.Position is not null)
                        {
                            headerCol.Item().AlignCenter().Text(picture.Position);
                        }
                    });
                    var imageContainer = filePage.Content().AlignCenter().AlignMiddle();
                    var degrees = DatabaseParsing.OrientationToDegrees(picture.Orientation);
                    for (int i = 0; i < degrees / 90; i++)
                    {
                        imageContainer = imageContainer.RotateLeft();
                    }
                    var sourceFilePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(picture.OriginalPath);
                    var imageBytes = fileSystem.File.ReadAllBytes(sourceFilePath);
                    imageContainer.Image(imageBytes).FitArea();
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

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System;
using FileDBShared.Validators;
using FileDBShared.FileFormats;
using FileDB.Model;
using System.IO.Abstractions;
using FileDB.ViewModel;
using FileDBInterface.Extensions;

namespace FileDB.Export.SearchResult;

enum TextType { Normal, Heading }

record TextLine(string Text, TextType Type);

public enum DescriptionPlacement { Heading, Subtitle }

public enum PicturesWithMetadataExporterFileType
{
    Png,
    Jpeg,
}

public class PicturesWithMetadataExporter(IFilesystemAccessProvider filesystemAccessProvider, DescriptionPlacement descriptionPlacement, IFileSystem fileSystem, PicturesWithMetadataExporterFileType fileType) : ISearchResultExporter
{
    private const int EmptyLineHeight = 15;
    private const int BackgroundMargin = 20;
    private const int BackgroundPadding = 20;
    private readonly SolidBrush textBgBrush = new(Color.FromArgb(229, 37, 37, 38));

    public void Export(SearchResultExport data, string path)
    {
        if (!fileSystem.Directory.Exists(path))
        {
            fileSystem.Directory.CreateDirectory(path);
        }

        int index = 1;
        foreach (var picture in data.Files.Where(x => x.FileType == FileType.Picture))
        {
            try
            {
                var bitmap = LoadImage(picture);
                var flipType = OrientationToFlipType(picture.Orientation);
                bitmap.RotateFlip(flipType);

                var textLines = CreateTextLines(data, picture);
                var subtitleLines = CreateSubtitleTextLines(picture);
                AddTextToImage(textLines, subtitleLines, bitmap);

                var fileExtension = fileType switch
                {
                    PicturesWithMetadataExporterFileType.Jpeg => ".jpg",
                    PicturesWithMetadataExporterFileType.Png => ".png",
                    _ => throw new NotImplementedException(),
                };

                var imageFormat = fileType switch
                {
                    PicturesWithMetadataExporterFileType.Jpeg => ImageFormat.Jpeg,
                    PicturesWithMetadataExporterFileType.Png => ImageFormat.Png,
                    _ => throw new NotImplementedException(),
                };

                var destFilePath = Path.Combine(path, $"{index}{fileExtension}");
                SaveBitmap(bitmap, destFilePath, imageFormat);

                index++;
            }
            catch
            {
                // Ignore picture
            }
        }
    }

    private static RotateFlipType OrientationToFlipType(int? orientation)
    {
        return orientation switch
        {
            1 => RotateFlipType.RotateNoneFlipNone,
            2 => RotateFlipType.RotateNoneFlipNone, // Flip not supported
            3 => RotateFlipType.Rotate180FlipNone,
            4 => RotateFlipType.RotateNoneFlipNone, // Flip not supported
            5 => RotateFlipType.RotateNoneFlipNone, // Flip not supported
            6 => RotateFlipType.Rotate270FlipNone,
            7 => RotateFlipType.RotateNoneFlipNone, // Flip not supported
            8 => RotateFlipType.Rotate90FlipNone,
            _ => RotateFlipType.RotateNoneFlipNone,
        };
    }

    private List<TextLine> CreateTextLines(SearchResultExport data, ExportedFile file)
    {
        var textLines = new List<TextLine>();

        var pictureDateText = string.Empty;
        if (file.Datetime is not null)
        {
            pictureDateText = HtmlExporter.CreateExportedFileDatetime(file.Datetime);
        }
        var pictureDescription = string.Empty;
        if (file.Description is not null && descriptionPlacement == DescriptionPlacement.Heading)
        {
            if (pictureDateText != string.Empty)
            {
                pictureDescription += ": ";
            }
            pictureDescription += file.Description;
        }
        if (pictureDateText.HasContent() || pictureDescription.HasContent())
        {
            textLines.Add(new TextLine($@"{pictureDateText}{pictureDescription}", TextType.Heading));
        }

        if (file.PersonIds.Count > 0)
        {
            var persons = data.Persons.Where(x => file.PersonIds.Contains(x.Id));
            var personStrings = FileTextOverlayCreator.GetPersonsTexts(file.Datetime, persons);
            textLines.AddRange(personStrings.Select(x => new TextLine(x, TextType.Normal)));
        }

        if (file.LocationIds.Count > 0)
        {
            var locations = data.Locations.Where(x => file.LocationIds.Contains(x.Id));
            var locationStrings = FileTextOverlayCreator.GetLocationsTexts(locations);
            textLines.AddRange(locationStrings.Select(x => new TextLine(x, TextType.Normal)));
        }

        if (file.TagIds.Count > 0)
        {
            var tags = data.Tags.Where(x => file.TagIds.Contains(x.Id));
            var tagStrings = FileTextOverlayCreator.GetTagsTexts(tags);
            textLines.AddRange(tagStrings.Select(x => new TextLine(x, TextType.Normal)));
        }

        return textLines;
    }

    private List<TextLine> CreateSubtitleTextLines(ExportedFile file)
    {
        var textLines = new List<TextLine>();

        if (file.Description is not null && descriptionPlacement == DescriptionPlacement.Subtitle)
        {
            var lines = file.Description.Split(FileModelValidator.DescriptionLineEnding);
            foreach (var line in lines)
            {
                textLines.Add(new TextLine(line, TextType.Normal));
            }
        }

        return textLines;
    }

    private Bitmap LoadImage(ExportedFile file)
    {
        var sourceFilePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
        using var fileStream = filesystemAccessProvider.FilesystemAccess.FileSystem.File.Open(sourceFilePath, FileMode.Open);
        return new Bitmap(fileStream);
    }

    private void AddTextToImage(List<TextLine> textLines, List<TextLine> subtitleLines, Bitmap bitmap)
    {
        using Graphics graphics = Graphics.FromImage(bitmap);
        const int AdaptForMinScreenHeight = 1080;
        var textSize = (int)Math.Round(Math.Max(bitmap.Height, AdaptForMinScreenHeight) / 60.0);
        using var headingFont = new Font("Arial", textSize, FontStyle.Bold, GraphicsUnit.Pixel);
        using var font = new Font("Arial", textSize, GraphicsUnit.Pixel);

        if (textLines.Count > 0)
        {
            var (rect, lineHeight) = MeasureLines(textLines, graphics, font, headingFont);
            rect.X += BackgroundMargin;
            rect.Y += BackgroundMargin;
            rect.Width += 2 * BackgroundPadding;
            rect.Height += 2 * BackgroundPadding;

            DrawLines(textLines, graphics, font, headingFont, rect, lineHeight);
        }

        if (subtitleLines.Count > 0)
        {
            var (rect, lineHeight) = MeasureLines(subtitleLines, graphics, font, headingFont);
            rect.Width += 2 * BackgroundPadding;
            rect.Height += 2 * BackgroundPadding;
            rect.X = ((int)graphics.VisibleClipBounds.Width - rect.Width) / 2;
            rect.Y = (int)graphics.VisibleClipBounds.Height - rect.Height - BackgroundMargin;

            DrawLines(subtitleLines, graphics, font, headingFont, rect, lineHeight);
        }
    }

    private (Rectangle rect, int lineHeight) MeasureLines(List<TextLine> lines, Graphics graphics, Font font, Font headingFont)
    {
        var rect = new Rectangle();
        int lineHeight = 0;

        foreach (var line in lines)
        {
            if (line.Text != string.Empty)
            {
                var size = graphics.MeasureString(line.Text, line.Type == TextType.Normal ? font : headingFont);
                if (size.Width > rect.Width)
                {
                    rect.Width = (int)size.Width;
                }
                rect.Height += (int)size.Height;
                lineHeight = (int)size.Height;
            }
            else
            {
                rect.Height += EmptyLineHeight;
            }
        }

        return (rect, lineHeight);
    }

    private void DrawLines(List<TextLine> lines, Graphics graphics, Font font, Font headingFont, Rectangle rect, int lineHeight)
    {
        graphics.FillRectangle(textBgBrush, rect);

        var pos = new PointF(rect.X + BackgroundPadding, rect.Y + BackgroundPadding);
        foreach (var line in lines)
        {
            if (line.Text != string.Empty)
            {
                graphics.DrawString(line.Text, line.Type == TextType.Normal ? font : headingFont, Brushes.White, pos);
                pos.Y += lineHeight;
            }
            else
            {
                pos.Y += EmptyLineHeight;
            }
        }
    }

    private void SaveBitmap(Bitmap bitmap, string path, ImageFormat imageFormat)
    {
        using var fileStream = fileSystem.FileStream.New(path, FileMode.CreateNew);
        bitmap.Save(fileStream, imageFormat);
    }
}

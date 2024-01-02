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
using FileDB.Extensions;
using FileDB.ViewModel;

namespace FileDB.Export.SearchResult;

enum TextType { Normal, Heading }

record TextLine(string Text, TextType Type);

public enum DescriptionPlacement { Heading, Subtitle }

public class PicturesWithMetadataExporter(IFilesystemAccessProvider filesystemAccessProvider, DescriptionPlacement descriptionPlacement, IFileSystem fileSystem) : ISearchResultExporter
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
        var pictures = data.Files.Where(x => FileTypeUtils.GetFileType(x.OriginalPath) == FileType.Picture);
        foreach (var picture in pictures)
        {
            var bitmap = LoadBitmap(picture);
            if (bitmap is not null)
            {
                if (picture.Orientation is not null)
                {
                    // TODO: add support for flipped values
                    switch (picture.Orientation)
                    {
                        case 1:
                            // Do nothing
                            break;
                        case 2:
                            break;
                        case 3:
                            bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case 4:
                            break;
                        case 5:
                            break;
                        case 6:
                            bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        case 7:
                            break;
                        case 8:
                            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                    }
                }

                var textLines = CreateTextLines(data, picture);
                var subtitleLines = CreateSubtitleTextLines(picture);
                AddTextToImage(textLines, subtitleLines, bitmap);

                var destFilePath = Path.Combine(path, $"{index}.png");
                SaveBitmap(bitmap, destFilePath);

                index++;
            }
        }
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

    private Bitmap? LoadBitmap(ExportedFile file)
    {
        try
        {
            var sourceFilePath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
            // TODO: how to load bitmap from FileSystem?
            return new Bitmap(sourceFilePath);
        }
        catch
        {
            return null;
        }
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

    private void SaveBitmap(Bitmap bitmap, string path)
    {
        // TODO: how to save bitmap to FileSystem?
        bitmap.Save(path, ImageFormat.Png);
    }
}

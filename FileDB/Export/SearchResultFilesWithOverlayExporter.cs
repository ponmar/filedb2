using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.Export
{
    enum TextType { Normal, Heading }

    class TextLine
    {
        public string Text { get; set; } = string.Empty;
        public TextType Type { get; set; } = TextType.Normal;
    }

    public class SearchResultFilesWithOverlayExporter : ISearchResultExporter
    {
        public void Export(SearchResultFileFormat data, string path)
        {
            var subdir = Path.Combine(path, "filesWithData");
            if (!Directory.Exists(subdir))
            {
                Directory.CreateDirectory(subdir);
            }

            int index = 1;
            foreach (var file in data.Files)
            {
                var bitmap = LoadBitmap(file);
                if (bitmap != null)
                {
                    var textLines = CreateTextLines(data, file);
                    AddTextToImage(textLines, bitmap);

                    var destFilePath = Path.Combine(subdir, $"{index}.png");
                    SaveBitmap(bitmap, destFilePath);

                    index++;
                }
            }
        }

        private List<TextLine> CreateTextLines(SearchResultFileFormat data, ExportedFile file)
        {
            var textLines = new List<TextLine>();

            var pictureDateText = string.Empty;
            if (file.Datetime != null)
            {
                pictureDateText = $"{SearchResultHtmlExporter.CreateExportedFileDatetime(file.Datetime)}";
            }
            var pictureDescription = string.Empty;
            if (file.Description != null)
            {
                if (pictureDateText != string.Empty)
                {
                    pictureDescription += ": ";
                }
                pictureDescription += $"{file.Description}";
            }
            textLines.Add(new TextLine() { Text = $@"{pictureDateText}{pictureDescription}", Type = TextType.Heading });

            if (file.PersonIds.Count > 0)
            {
                textLines.Add(new());
                var persons = file.PersonIds.Select(x => data.Persons.First(y => y.Id == x));
                var personStrings = persons.Select(x => $"{x.Firstname} {x.Lastname}{Utils.GetPersonAgeInFileString(file.Datetime, x.DateOfBirth)}").ToList();
                personStrings.Sort();
                textLines.AddRange(personStrings.Select(x => new TextLine() { Text = x }));
            }

            if (file.LocationIds.Count > 0)
            {
                textLines.Add(new());
                var locations = file.LocationIds.Select(x => data.Locations.First(y => y.Id == x));
                var locationStrings = locations.Select(x => x.Name).ToList();
                locationStrings.Sort();
                textLines.AddRange(locationStrings.Select(x => new TextLine() { Text = x }));
            }

            if (file.TagIds.Count > 0)
            {
                textLines.Add(new());
                var tags = file.TagIds.Select(x => data.Tags.First(y => y.Id == x));
                var tagStrings = tags.Select(x => x.Name).ToList();
                tagStrings.Sort();
                textLines.AddRange(tagStrings.Select(x => new TextLine() { Text = x }));
            }

            return textLines;
        }

        private Bitmap LoadBitmap(ExportedFile file)
        {
            try
            {
                var sourceFilePath = Model.Model.Instance.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
                return new Bitmap(sourceFilePath);
            }
            catch
            {
                return null;
            }
        }

        private void AddTextToImage(List<TextLine> textLines, Bitmap bitmap)
        {
            using Graphics graphics = Graphics.FromImage(bitmap);
            using var headingFont = new Font("Arial", 18, FontStyle.Bold);
            using var font = new Font("Arial", 18);

            var rectPadding = 20;
            var bgRect = new Rectangle(10, 10, 2 * rectPadding, 2 * rectPadding);

            int emptyLineHeight = 15;
            int lineHeight = 0;
            foreach (var textLine in textLines)
            {
                if (textLine.Text != string.Empty)
                {
                    var size = graphics.MeasureString(textLine.Text, textLine.Type == TextType.Normal ? font : headingFont);
                    if (size.Width + 2 * rectPadding > bgRect.Width)
                    {
                        bgRect.Width = (int)size.Width + 2 * rectPadding;
                    }
                    bgRect.Height += (int)size.Height;
                    lineHeight = (int)size.Height;
                }
                else
                {
                    bgRect.Height += emptyLineHeight;
                }
            }

            var bgColor = Color.FromArgb(229, 37, 37, 38);
            var textBgBrush = new SolidBrush(bgColor);

            graphics.FillRectangle(textBgBrush, bgRect);

            var pos = new PointF(bgRect.X + rectPadding, bgRect.Y + rectPadding);
            foreach (var textLine in textLines)
            {
                if (textLine.Text != string.Empty)
                {
                    graphics.DrawString(textLine.Text, textLine.Type == TextType.Normal ? font : headingFont, Brushes.White, pos);
                    pos.Y += lineHeight;
                }
                else
                {
                    pos.Y += emptyLineHeight;
                }
            }
        }

        private void SaveBitmap(Bitmap bitmap, string path)
        {
            bitmap.Save(path, ImageFormat.Png);
        }
    }
}

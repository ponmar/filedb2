using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace FileDB.Export
{
    public class FilesWithDataExporter : IExporter
    {
        public void Export(DataFileFormat data, string path)
        {
            var model = Model.Model.Instance;
            var subdir = Path.Combine(path, "filesWithData");
            if (!Directory.Exists(subdir))
            {
                Directory.CreateDirectory(subdir);
            }

            int index = 1;
            foreach (var file in data.Files)
            {
                // TODO: ignore non-image files?
                var sourceFilePath = model.FilesystemAccess.ToAbsolutePath(file.OriginalPath);
                // TODO: throws ArgumentException for formats not supported?
                var bitmap = new Bitmap(sourceFilePath);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    using var font = new Font("Arial", 18);

                    var textLines = new List<string>();
                    textLines.Add("testing");

                    var rectPadding = 20;
                    var rectMargin = 20;

                    var bgRect = new Rectangle(rectMargin, rectMargin, 2*rectPadding, 2*rectPadding);

                    int lineHeight = 0;
                    foreach (var textLine in textLines)
                    {
                        var size = graphics.MeasureString(textLine, font);
                        if (size.Width + 2*rectPadding > bgRect.Width)
                        {
                            bgRect.Width = (int)size.Width + 2*rectPadding;
                        }
                        bgRect.Height += (int)size.Height;
                        lineHeight = (int)size.Height;
                    }

                    var bgColor = Color.FromArgb(229, 37, 37, 38);
                    var textBgBrush = new SolidBrush(bgColor);
                    
                    graphics.FillRectangle(textBgBrush, bgRect);

                    var pos = new PointF(bgRect.X + rectPadding, bgRect.Y + rectPadding);
                    foreach (var textLine in textLines)
                    {
                        graphics.DrawString(textLine, font, Brushes.White, pos);
                        pos.Y += lineHeight;
                    }
                }

                var destFilePath = Path.Combine(subdir, $"{index}.png");
                bitmap.Save(destFilePath, ImageFormat.Png);
                index++;
            }
        }
    }
}

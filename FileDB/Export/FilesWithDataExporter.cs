using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

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
                    using var font = new Font("Arial", 10);
                    graphics.DrawString("testing", font, Brushes.Blue, new PointF(10f, 10f));
                    //graphics.MeasureString();
                    //graphics.DrawRectangle();
                }

                var destFilePath = Path.Combine(subdir, $"{index}.png");
                bitmap.Save(destFilePath, ImageFormat.Png);
                index++;
            }
        }
    }
}

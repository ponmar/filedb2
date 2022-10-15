using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;
using log4net;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;

namespace FileDBInterface.FilesystemAccess
{
    public class FilesystemAccess : IFilesystemAccess
    {
        private static readonly ILog log = LogManager.GetLogger(nameof(FilesystemAccess));

        private readonly string filesRootDirectory;

        public FilesystemAccess(string filesRootDirectory)
        {
            this.filesRootDirectory = filesRootDirectory;
            if (System.IO.Directory.Exists(filesRootDirectory))
            {
                log.Info($"Using files root directory: {filesRootDirectory}");
            }
            else
            {
                log.Warn($"Files root directory does not exist: {filesRootDirectory}");
            }
        }

        public IEnumerable<string> ListNewFilesystemFiles(string path, IEnumerable<string> blacklistedFilePathPatterns, IEnumerable<string> whitelistedFilePathPatterns, bool includeHiddenDirectories, IFilesAccess filesDbAccess)
        {
            if (!path.StartsWith(filesRootDirectory))
            {
                yield break;
            }

            foreach (var filename in System.IO.Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            {
                var internalPath = ToInternalFilesPath(filename);
                if (PathIsApplicable(internalPath, blacklistedFilePathPatterns, whitelistedFilePathPatterns, includeHiddenDirectories) &&
                    // TODO: optimize by adding check if path exists
                    filesDbAccess.GetFileByPath(internalPath) == null)
                {
                    yield return internalPath;
                }
            }
        }

        public bool PathIsApplicable(string internalPath, IEnumerable<string> blacklistedFilePathPatterns, IEnumerable<string> whitelistedFilePathPatterns, bool includeHiddenDirectories)
        {
            return !PathIsBlacklisted(internalPath, blacklistedFilePathPatterns) &&
                    PathIsWhitelisted(internalPath, whitelistedFilePathPatterns) &&
                    PathIsVisible(internalPath, includeHiddenDirectories);
        }

        private bool PathIsBlacklisted(string internalPath, IEnumerable<string> blacklistedFilePathPatterns)
        {
            return blacklistedFilePathPatterns.FirstOrDefault(pattern => internalPath.IndexOf(pattern) != -1) != null;
        }

        private bool PathIsWhitelisted(string internalPath, IEnumerable<string> whitelistedFilePathPatterns)
        {
            if (whitelistedFilePathPatterns.Count() == 0)
                return true;

            var pathLower = internalPath.ToLower();
            return whitelistedFilePathPatterns.FirstOrDefault(pattern => pathLower.EndsWith(pattern)) != null;
        }

        private bool PathIsVisible(string internalPath, bool includeHiddenDirectories)
        {
            return includeHiddenDirectories || !PathIsHidden(internalPath);
        }

        private bool PathIsHidden(string internalPath)
        {
            return internalPath.StartsWith('.') || internalPath.IndexOf("/.") != -1;
        }

        public IEnumerable<string> ListAllFilesystemDirectories()
        {
            var dirs = System.IO.Directory.GetDirectories(filesRootDirectory, "*.*", SearchOption.AllDirectories);
            return dirs.Select(p => ToInternalFilesPath(p));
        }

        public IEnumerable<FilesModel> GetFilesMissingInFilesystem(IEnumerable<FilesModel> allFiles)
        {
            foreach (var file in allFiles)
            {
                if (!File.Exists(ToAbsolutePath(file.Path)))
                {
                    yield return file;
                }
            }
        }

        public string ToAbsolutePath(string internalPath)
        {
            var path = Path.Join(filesRootDirectory, internalPath);
            return path.Replace('\\', '/');
        }

        private string ToInternalFilesPath(string path)
        {
            if (path.StartsWith(filesRootDirectory))
            {
                path = path.Substring(filesRootDirectory.Length);
            }
            return FixInternalPath(path);
        }

        private string FixInternalPath(string internalPath)
        {
            internalPath = internalPath.Replace('\\', '/');
            while (internalPath.StartsWith('/'))
            {
                internalPath = internalPath.Substring(1);
            }
            return internalPath;
        }

        public FileMetadata ParseFileMetadata(string path)
        {
            string? datetime = null;
            string? position = null;
            int? orientation = null;

            if (FileTypeSupportsExif(path))
            {
                ParseFileExif(path, out var dateTaken, out var location, out orientation);

                if (dateTaken != null)
                {
                    datetime = DatabaseParsing.DateTakenToFilesDatetime(dateTaken.Value);
                }

                if (location != null)
                {
                    position = DatabaseParsing.ToFilesPosition(location.Latitude, location.Longitude);
                }
            }

            if (datetime == null)
            {
                datetime = DatabaseParsing.PathToFilesDatetime(path);
            }

            return new FileMetadata(path, datetime, position, orientation);
        }

        private bool FileTypeSupportsExif(string path)
        {
            var fileExtension = Path.GetExtension(path);
            return fileExtension switch
            {
                ".jpg" or ".jpeg" or ".JPG" or ".JPEG" => true,
                _ => false,
            };
        }

        private void ParseFileExif(string path, out DateTime? dateTaken, out GeoLocation? location, out int? orientation)
        {
            dateTaken = null;
            location = null;
            orientation = null;

            try
            {
                var directories = ImageMetadataReader.ReadMetadata(path);

                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                dateTaken = subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);

                var gpsDir = directories.OfType<GpsDirectory>().FirstOrDefault();
                location = gpsDir?.GetGeoLocation();

                if (TryGetOrientationTag(directories, out var orientationTag) &&
                    orientationTag >= 1 && orientationTag <= 8)
                {
                    orientation = orientationTag;
                }
            }
            catch (IOException)
            {
            }
            catch (ImageProcessingException)
            {
            }
            catch (MetadataException)
            {
            }
        }

        private static bool TryGetOrientationTag(IEnumerable<MetadataExtractor.Directory> directories, out int? orientation)
        {
            orientation = null;
            var ifd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (ifd0Directory != null)
            {
                orientation = ifd0Directory.TryGetInt32(ExifDirectoryBase.TagOrientation, out int value) ? value : null;
            }
            return orientation != null;
        }
    }
}

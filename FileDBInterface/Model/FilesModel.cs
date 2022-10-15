namespace FileDBInterface.Model
{
    public class FilesModel
    {
        public int Id { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Path { get; set; } // Format: path/to/file/filename
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string? Description { get; set; }
        public string? Datetime { get; set; } // Format: YYYY, YYYY-MM-DD or YYYY-MM-DDTHH:MM:SS
        public string? Position { get; set; } // Format: <latitude> <longitude>
        public int? Orientation { get; set; } // Format: null (no orientation set), 1-8 according to Exif
    }
}

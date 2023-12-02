namespace FileDBShared.Model;

public class FileModel
{
    public required int Id { get; set; }
    public required string Path { get; set; } // Format: path/to/file/filename
    public string? Description { get; set; }
    public string? Datetime { get; set; } // Format: YYYY, YYYY-MM, YYYY-MM-DD or YYYY-MM-DDTHH:MM:SS
    public string? Position { get; set; } // Format: <latitude> <longitude>
    public int? Orientation { get; set; } // Format: null (no orientation set), 1-8 according to Exif
}

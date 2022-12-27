namespace FileDBShared.Model;

public class LocationModel
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Position { get; set; } // Format: <latitude> <longitude>
}

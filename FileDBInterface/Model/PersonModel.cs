namespace FileDBInterface.Model;

public class PersonModel
{
    public required int Id { get; set; }
    public required string ShortName { get; set; }
    public required string FullName { get; set; }
    public string? Description { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Deceased { get; set; }
    public int? ProfileFileId { get; set; }
    public Sex Sex { get; set; }
}

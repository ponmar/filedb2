namespace FileDBInterface.Model;

public class PersonModel
{
    public required int Id { get; set; }
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public string? Description { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Deceased { get; set; }
    public int? ProfileFileId { get; set; }
    public Sex Sex { get; set; }
}

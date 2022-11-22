namespace FileDBInterface.Model
{
    // Values from: https://en.wikipedia.org/wiki/ISO/IEC_5218
    public enum Sex
    {
        NotKnown = 0,
        Male = 1,
        Female = 2,
        NotApplicable = 9,
    }

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
}

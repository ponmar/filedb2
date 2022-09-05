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
        public int Id { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Firstname { get; set; }
        public string Lastname { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string? Description { get; set; }
        public string? DateOfBirth { get; set; } // Format: YYYY-MM-DD
        public string? Deceased { get; set; } // Format: YYYY-MM-DD
        public int? ProfileFileId { get; set; }
        public Sex Sex { get; set; }
    }
}

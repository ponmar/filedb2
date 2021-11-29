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
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Description { get; set; }
        public string DateOfBirth { get; set; } // Format: YYYY-MM-DD
        public string Deceased { get; set; } // Format: YYYY-MM-DD
        public int? ProfileFileId { get; set; }
        public Sex Sex { get; set; }
    }
}

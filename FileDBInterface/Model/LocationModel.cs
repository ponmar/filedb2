namespace FileDBInterface.Model
{
    public class LocationModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Position { get; set; } // Format: <latitude> <longitude>
    }
}

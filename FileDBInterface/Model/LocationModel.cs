namespace FileDBInterface.Model
{
    public class LocationModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string position { get; set; } // Format: <latitude> <longitude>
    }
}

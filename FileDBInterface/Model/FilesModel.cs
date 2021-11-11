namespace FileDBInterface.Model
{
    public class FilesModel
    {
        public int id { get; set; }
        public string path { get; set; } // Format: path/to/file/filename
        public string description { get; set; }
        public string datetime { get; set; } // Format: YYYY, YYYY-MM-DD or YYYY-MM-DDTHH:MM:SS
        public string position { get; set; } // Format: <latitude> <longitude>
    }
}

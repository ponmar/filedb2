namespace FileDBInterface.Model
{
    public class FilesModel
    {
        public int Id { get; set; }
        public string Path { get; set; } // Format: path/to/file/filename
        public string Description { get; set; }
        public string Datetime { get; set; } // Format: YYYY, YYYY-MM-DD or YYYY-MM-DDTHH:MM:SS
        public string Position { get; set; } // Format: <latitude> <longitude>
    }
}

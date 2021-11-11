namespace FileDBInterface
{
    public class FileDBConfig
    {
        public string Database { get; set; }
        public string FilesRootDirectory { get; set; }
        public double FileToLocationMaxDistance { get; set; } = 300;
    }
}

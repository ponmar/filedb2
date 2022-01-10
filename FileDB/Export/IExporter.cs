namespace FileDB.Export
{
    public interface IExporter
    {
        void Export(DataFileFormat data, string path);
    }
}

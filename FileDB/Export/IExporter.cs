namespace FileDB.Export
{
    public interface IExporter
    {
        void Export(ExportedData data, string path);
    }
}

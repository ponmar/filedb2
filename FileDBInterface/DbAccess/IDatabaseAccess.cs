namespace FileDBInterface.DbAccess
{
    public interface IDatabaseAccess : IPersonAccess, ILocationAccess, ITagAccess, IFilesystemAccess, IFilesAccess
    {
        public string ToAbsolutePath(string internalPath);
    }
}

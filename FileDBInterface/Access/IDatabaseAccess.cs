namespace FileDBInterface.Access
{
    public interface IDatabaseAccess : IPersonAccess, ILocationAccess, ITagAccess, IFilesystemAccess, IFilesAccess
    {
        public string ToAbsolutePath(string internalPath);
    }
}

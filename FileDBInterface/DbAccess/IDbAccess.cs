namespace FileDBInterface.DbAccess;

public interface IDbAccess : IPersonAccess, ILocationAccess, ITagAccess, IFilesAccess
{
    string Database { get; set; }
}

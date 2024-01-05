using FileDBInterface.DbAccess.SQLite;
using System.Collections.Generic;

namespace FileDBInterface.DbAccess;

public interface IDbAccess : IPersonAccess, ILocationAccess, ITagAccess, IFilesAccess
{
    List<DbMigrationResult> DbMigrations { get; }
}

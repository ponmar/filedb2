using FileDBInterface.DatabaseAccess.SQLite;
using System.Collections.Generic;

namespace FileDBInterface.DatabaseAccess;

public interface IDatabaseAccess : IPersonAccess, ILocationAccess, ITagAccess, IFilesAccess
{
    bool NeedsMigration { get; }

    List<DatabaseMigrationResult> Migrate();
}

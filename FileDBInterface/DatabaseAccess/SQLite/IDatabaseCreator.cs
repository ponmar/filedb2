using System.Data;

namespace FileDBInterface.DatabaseAccess.SQLite;

public interface IDatabaseCreator
{
    void CreateDatabase(string databasePath);

    IDbConnection CreateInMemory();
}

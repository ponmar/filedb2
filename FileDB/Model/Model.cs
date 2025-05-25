using FileDB.Configuration;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;

namespace FileDB.Model;

public interface IDatabaseAccessProvider
{
    IDatabaseAccess DbAccess { get; }
}

public interface IFilesystemAccessProvider
{
    IFilesystemAccess FilesystemAccess { get; }
}

public interface IConfigProvider
{
    ApplicationFilePaths FilePaths { get; }
    Config Config { get; }
}

public interface IConfigUpdater
{
    void InitConfig(ApplicationFilePaths applicationFilePaths, Config config, IDatabaseAccess dbAccess, IFilesystemAccess filesystemAccess);
    void UpdateConfig(Config config);
    void UpdateDatabaseAccess(IDatabaseAccess dbAccess);
}

public record ApplicationFilePaths(string FilesRootDir, string ConfigPath, string DatabasePath);

public class Model : IConfigProvider, IConfigUpdater, IDatabaseAccessProvider, IFilesystemAccessProvider
{
    public IDatabaseAccess DbAccess { get; private set; } = new NoDatabaseAccess();
    public IFilesystemAccess FilesystemAccess { get; private set; }
    public ApplicationFilePaths FilePaths { get; private set; }

    public Config Config { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Model()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }
    
    public void InitConfig(ApplicationFilePaths filePaths, Config config, IDatabaseAccess dbAccess, IFilesystemAccess filesystemAccess)
    {
        FilePaths = filePaths;
        Config = config;
        DbAccess = dbAccess;
        FilesystemAccess = filesystemAccess;
        Messenger.Send<ConfigUpdated>();
    }

    public void UpdateConfig(Config config)
    {
        Config = config;
        Messenger.Send<ConfigUpdated>();
    }

    public void UpdateDatabaseAccess(IDatabaseAccess dbAccess)
    {
        DbAccess = dbAccess;
        Messenger.Send<ConfigUpdated>();
    }
}

using FileDB.Configuration;
using FileDB.Model;
using FileDBInterface.DbAccess;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace FileDB.Notifiers;

public interface INotifierFactory
{
    List<INotifier> GetContinousNotifiers(Config config, IDbAccess dbAccess);
    List<INotifier> GetStartupNotifiers(Config config, IDbAccess dbAccess);
}

public class NotifierFactory : INotifierFactory
{
    public List<INotifier> GetContinousNotifiers(Config config, IDbAccess dbAccess)
    {
        var notifiers = new List<INotifier>();
        var persons = dbAccess.GetPersons();

        if (config.BirthdayReminder)
        {
            notifiers.Add(new BirthdayNotifier(persons, BirthdayNotificationFor.Alive));
        }

        if (config.BirthdayReminderForDeceased)
        {
            notifiers.Add(new BirthdayNotifier(persons, BirthdayNotificationFor.Deceased));
        }

        if (config.RipReminder)
        {
            notifiers.Add(new RestInPeaceNotifier(persons));
        }

        return notifiers;
    }

    public List<INotifier> GetStartupNotifiers(Config config, IDbAccess dbAccess)
    {
        var notifiers = new List<INotifier>();

        if (config.BackupReminder)
        {
            notifiers.Add(new BackupNotifier(new DatabaseBackup(ServiceLocator.Resolve<IConfigRepository>(), ServiceLocator.Resolve<IFileSystem>()).ListAvailableBackupFiles(), 30));
        }

        if (config.MissingFilesRootDirNotification)
        {
            notifiers.Add(new MissingFilesRootDirNotifier(config.FilesRootDirectory));
        }

        if (config.CacheFiles)
        {
            var configDir = new AppDataConfig<Config>(Utils.ApplicationName, ServiceLocator.Resolve<IFileSystem>()).ConfigDirectory;
            var cacheDir = Path.Combine(configDir, DefaultConfigs.CacheSubdir);
            var cacheFileIds = dbAccess.GetPersons().Where(x => x.ProfileFileId != null).Select(x => x.ProfileFileId!.Value);
            notifiers.Add(new CacheNotifier(cacheDir, cacheFileIds));
        }

        return notifiers;
    }
}

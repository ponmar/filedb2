using FileDB.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileDB.Notifiers;

public interface INotifierFactory
{
    List<INotifier> GetContinousNotifiers();
    List<INotifier> GetStartupNotifiers();
}

public class NotifierFactory : INotifierFactory
{
    private readonly Model.Model model = Model.Model.Instance;

    public List<INotifier> GetContinousNotifiers()
    {
        var notifiers = new List<INotifier>();
        var persons = model.DbAccess.GetPersons();

        if (model.Config.BirthdayReminder)
        {
            notifiers.Add(new BirthdayNotifier(persons, BirthdayNotificationFor.Alive));
        }

        if (model.Config.BirthdayReminderForDeceased)
        {
            notifiers.Add(new BirthdayNotifier(persons, BirthdayNotificationFor.Deceased));
        }

        if (model.Config.RipReminder)
        {
            notifiers.Add(new RestInPeaceNotifier(persons));
        }

        return notifiers;
    }

    public List<INotifier> GetStartupNotifiers()
    {
        var notifiers = new List<INotifier>();

        if (model.Config.BackupReminder)
        {
            notifiers.Add(new BackupNotifier(new DatabaseBackup().ListAvailableBackupFiles(), 30));
        }

        if (model.Config.MissingFilesRootDirNotification)
        {
            notifiers.Add(new MissingFilesRootDirNotifier(model.Config.FilesRootDirectory));
        }

        if (model.Config.CacheFiles)
        {
            var configDir = new AppDataConfig<Config>(Utils.ApplicationName).ConfigDirectory;
            var cacheDir = Path.Combine(configDir, DefaultConfigs.CacheSubdir);
            var cacheFileIds = model.DbAccess.GetPersons().Where(x => x.ProfileFileId != null).Select(x => x.ProfileFileId!.Value);
            notifiers.Add(new CacheNotifier(cacheDir, cacheFileIds));
        }

        return notifiers;
    }
}

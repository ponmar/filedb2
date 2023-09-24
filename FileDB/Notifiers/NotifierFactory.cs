using FileDB.Configuration;
using FileDB.Model;
using FileDBInterface.DbAccess;
using System.Collections.Generic;
using System.IO.Abstractions;

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
            notifiers.Add(new BackupNotifier(new DatabaseBackup(
                ServiceLocator.Resolve<IFileSystem>(),
                ServiceLocator.Resolve<IConfigRepository>()).ListAvailableBackupFiles(), 30));
        }

        return notifiers;
    }
}

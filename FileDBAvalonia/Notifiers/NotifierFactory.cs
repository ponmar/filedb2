using FileDBAvalonia.Configuration;
using FileDBAvalonia.Model;
using FileDBInterface.DatabaseAccess;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace FileDBAvalonia.Notifiers;

public interface INotifierFactory
{
    List<INotifier> GetContinousNotifiers(Config config, IDatabaseAccess dbAccess);
    List<INotifier> GetStartupNotifiers(Config config, IDatabaseAccess dbAccess);
}

public class NotifierFactory : INotifierFactory
{
    public List<INotifier> GetContinousNotifiers(Config config, IDatabaseAccess dbAccess)
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

    public List<INotifier> GetStartupNotifiers(Config config, IDatabaseAccess dbAccess)
    {
        var notifiers = new List<INotifier>();

        if (config.BackupReminder)
        {
            var fileSystem = ServiceLocator.Resolve<IFileSystem>();
            var configProvider = ServiceLocator.Resolve<IConfigProvider>();
            notifiers.Add(new BackupNotifier(new FileBackup(fileSystem, configProvider.FilePaths.DatabasePath), 30));
        }

        notifiers.Add(new MissingDatabaseNotifier(
            ServiceLocator.Resolve<IConfigProvider>().FilePaths,
            ServiceLocator.Resolve<IFileSystem>()));

        return notifiers;
    }
}

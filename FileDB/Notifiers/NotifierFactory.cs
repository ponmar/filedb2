﻿using FileDB.Configuration;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace FileDB.Notifiers;

public interface INotifierFactory
{
    IEnumerable<INotifier> GetContinousNotifiers(Config config, IDatabaseAccess dbAccess);
    IEnumerable<INotifier> GetStartupNotifiers(Config config, IDatabaseAccess dbAccess);
}

public class NotifierFactory : INotifierFactory
{
    public IEnumerable<INotifier> GetContinousNotifiers(Config config, IDatabaseAccess dbAccess)
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

    public IEnumerable<INotifier> GetStartupNotifiers(Config config, IDatabaseAccess dbAccess)
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

        notifiers.Add(new DemoNotifier(ServiceLocator.Resolve<IConfigProvider>()));

        return notifiers;
    }
}

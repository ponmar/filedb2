﻿using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FileDBAvalonia.Dialogs;
using FileDBAvalonia.Model;
using FileDBAvalonia.Notifiers;
using FileDBAvalonia.ViewModels;
using FileDBAvalonia.ViewModels.Dialogs;
using FileDBAvalonia.ViewModels.Search;
using FileDBAvalonia.ViewModels.Search.File;
using System.IO.Abstractions;

namespace FileDBAvalonia;

public static class Bootstrapper
{
    public static WindsorContainer Container { get; private set; } = new WindsorContainer();

    private static void Bootstrap()
    {
        Container.Register(Component.For<IFileSystem>().ImplementedBy<FileSystem>());
        Container.Register(Component.For<INotifierFactory>().ImplementedBy<NotifierFactory>());
        Container.Register(Component.For<ISpeeker>().ImplementedBy<Speeker>());
        Container.Register(Component.For<DateObserver>().ImplementedBy<DateObserver>());
        Container.Register(Component.For<IClipboardService>().ImplementedBy<ClipboardService>());

        Container.Register(
            Component.For(
                typeof(IPersonsRepository),
                typeof(ILocationsRepository),
                typeof(ITagsRepository))
                    .ImplementedBy<DatabaseCache>());

        Container.Register(Component.For<AboutViewModel>().ImplementedBy<AboutViewModel>());
        Container.Register(Component.For<IDialogs>().ImplementedBy<Dialogs.Dialogs>());

        Container.Register(
            Component.For(
                typeof(INotificationHandling),
                typeof(INotificationsRepository),
                typeof(IConfigProvider),
                typeof(IDatabaseAccessProvider),
                typeof(IFilesystemAccessProvider),
                typeof(IConfigUpdater))
                    .ImplementedBy<Model.Model>());

        Container.Register(Component.For<MainViewModel>().ImplementedBy<MainViewModel>());
        Container.Register(Component.For(
            typeof(ResultViewModel),
            typeof(ISearchResultRepository),
            typeof(IFileSelector))
                .ImplementedBy<ResultViewModel>());
        Container.Register(Component.For<FileViewModel>().ImplementedBy<FileViewModel>());
        Container.Register(Component.For<NotificationsViewModel>().ImplementedBy<NotificationsViewModel>());
        Container.Register(Component.For<SettingsViewModel>().ImplementedBy<SettingsViewModel>());
        Container.Register(Component.For<LocationsViewModel>().ImplementedBy<LocationsViewModel>());
        Container.Register(Component.For<PersonsViewModel>().ImplementedBy<PersonsViewModel>());
        Container.Register(Component.For<TagsViewModel>().ImplementedBy<TagsViewModel>());
        Container.Register(Component.For<BirthdaysViewModel>().ImplementedBy<BirthdaysViewModel>());
        Container.Register(Component.For<RipViewModel>().ImplementedBy<RipViewModel>());
        Container.Register(Component.For<ToolsViewModel>().ImplementedBy<ToolsViewModel>());
        Container.Register(Component.For<FilesViewModel>().ImplementedBy<FilesViewModel>());
        Container.Register(Component.For<CriteriaViewModel>().ImplementedBy<CriteriaViewModel>());
        Container.Register(Component.For<FileCategorizationViewModel>().ImplementedBy<FileCategorizationViewModel>());
        Container.Register(Component.For<AddLocationViewModel>().ImplementedBy<AddLocationViewModel>().LifestyleTransient());
        Container.Register(Component.For<AddPersonViewModel>().ImplementedBy<AddPersonViewModel>().LifestyleTransient());
        Container.Register(Component.For<AddTagViewModel>().ImplementedBy<AddTagViewModel>().LifestyleTransient());

        Container.Register(Component.For<IImageLoader>().ImplementedBy<ImageLoader>());

        Container.Register(Component.For<ExportSearchResultViewModel>().ImplementedBy<ExportSearchResultViewModel>().LifestyleTransient());
        Container.Register(Component.For<BrowseSubDirectoriesViewModel>().ImplementedBy<BrowseSubDirectoriesViewModel>().LifestyleTransient());

        //Container.Register(Component.For<DateViewModel>().ImplementedBy<DateViewModel>().LifestyleTransient());
        //Container.Register(Component.For<AnnualDateViewModel>().ImplementedBy<AnnualDateViewModel>().LifestyleTransient());
    }

    public static void StartServices()
    {
        ServiceLocator.Resolve<DateObserver>();
    }

    public static void Reset()
    {
        Container = new WindsorContainer();
        Bootstrap();
    }
}

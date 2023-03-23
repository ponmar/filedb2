using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FileDB.Model;
using FileDB.Notifiers;
using FileDB.ViewModel;
using System.IO.Abstractions;

namespace FileDB;

public static class Bootstrapper
{
    public static WindsorContainer Container { get; private set; } = new WindsorContainer();

    public static void Bootstrap()
    {
        Container.Register(Component.For<IFileSystem>().ImplementedBy<FileSystem>());
        Container.Register(Component.For<IDialogs>().ImplementedBy<Dialogs>());
        Container.Register(Component.For<INotifierFactory>().ImplementedBy<NotifierFactory>());
        
        Container.Register(
            Component.For(
                typeof(INotificationHandling),
                typeof(IConfigRepository),
                typeof(IDbAccessRepository),
                typeof(IFilesystemAccessRepository),
                typeof(IConfigUpdater))
                    .ImplementedBy<Model.Model>());

        Container.Register(Component.For<AboutViewModel>().ImplementedBy<AboutViewModel>());
        Container.Register(Component.For<BirthdaysViewModel>().ImplementedBy<BirthdaysViewModel>());
        Container.Register(Component.For<FilesViewModel>().ImplementedBy<FilesViewModel>());
        Container.Register(Component.For<LocationsViewModel>().ImplementedBy<LocationsViewModel>());
        Container.Register(Component.For<NotificationsViewModel>().ImplementedBy<NotificationsViewModel>());
        Container.Register(Component.For<PersonsViewModel>().ImplementedBy<PersonsViewModel>());
        Container.Register(Component.For<RipViewModel>().ImplementedBy<RipViewModel>());
        Container.Register(Component.For<SettingsViewModel>().ImplementedBy<SettingsViewModel>());
        Container.Register(Component.For<TagsViewModel>().ImplementedBy<TagsViewModel>());
        Container.Register(Component.For<ToolsViewModel>().ImplementedBy<ToolsViewModel>());
        Container.Register(Component.For<FindViewModel>().ImplementedBy<FindViewModel>());
        Container.Register(Component.For<SearchResultViewModel>().ImplementedBy<SearchResultViewModel>());

        Container.Register(Component.For<AddLocationViewModel>().ImplementedBy<AddLocationViewModel>().LifestyleTransient());
        Container.Register(Component.For<AddPersonViewModel>().ImplementedBy<AddPersonViewModel>().LifestyleTransient());
        Container.Register(Component.For<AddTagViewModel>().ImplementedBy<AddTagViewModel>().LifestyleTransient());

        Container.Register(Component.For<BrowseDirectoriesViewModel>().ImplementedBy<BrowseDirectoriesViewModel>().LifestyleTransient());
        Container.Register(Component.For<CreateDatabaseViewModel>().ImplementedBy<CreateDatabaseViewModel>().LifestyleTransient());
        Container.Register(Component.For<ExportViewModel>().ImplementedBy<ExportViewModel>().LifestyleTransient());
    }

    public static void Reset()
    {
        Container = new WindsorContainer();
        Bootstrap();
    }
}

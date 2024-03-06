using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FileDBAvalonia.Dialogs;
using FileDBAvalonia.Model;
using FileDBAvalonia.Notifiers;
using FileDBAvalonia.ViewModels;
using System.IO.Abstractions;

namespace FileDBAvalonia;

public static class Bootstrapper
{
    public static WindsorContainer Container { get; private set; } = new WindsorContainer();

    public static void Bootstrap()
    {
        Container.Register(Component.For<IFileSystem>().ImplementedBy<FileSystem>());
        Container.Register(Component.For<INotifierFactory>().ImplementedBy<NotifierFactory>());

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

        Container.Register(Component.For<NotificationsViewModel>().ImplementedBy<NotificationsViewModel>());
        Container.Register(Component.For<SettingsViewModel>().ImplementedBy<SettingsViewModel>());
        Container.Register(Component.For<LocationsViewModel>().ImplementedBy<LocationsViewModel>());
        Container.Register(Component.For<PersonsViewModel>().ImplementedBy<PersonsViewModel>());
        Container.Register(Component.For<TagsViewModel>().ImplementedBy<TagsViewModel>());
        Container.Register(Component.For<BirthdaysViewModel>().ImplementedBy<BirthdaysViewModel>());
        Container.Register(Component.For<RipViewModel>().ImplementedBy<RipViewModel>());
        Container.Register(Component.For<ToolsViewModel>().ImplementedBy<ToolsViewModel>());
        Container.Register(Component.For<FilesViewModel>().ImplementedBy<FilesViewModel>());

        Container.Register(Component.For<IImageLoader>().ImplementedBy<ImageLoader>());

        /*
       
        Container.Register(Component.For<ISpeeker>().ImplementedBy<Speeker>());

        Container.Register(Component.For<MainViewModel>().ImplementedBy<MainViewModel>());
        Container.Register(Component.For<SearchViewModel>().ImplementedBy<SearchViewModel>());
        Container.Register(Component.For<SearchResultViewModel>().ImplementedBy<SearchResultViewModel>());
        Container.Register(Component.For<FileCategorizationViewModel>().ImplementedBy<FileCategorizationViewModel>());

        Container.Register(
            Component.For(
                typeof(SearchCriteriaViewModel),
                typeof(ISearchResultRepository))
                    .ImplementedBy<SearchCriteriaViewModel>());        

        Container.Register(Component.For<AddLocationViewModel>().ImplementedBy<AddLocationViewModel>().LifestyleTransient());
        Container.Register(Component.For<AddPersonViewModel>().ImplementedBy<AddPersonViewModel>().LifestyleTransient());
        Container.Register(Component.For<AddTagViewModel>().ImplementedBy<AddTagViewModel>().LifestyleTransient());

        Container.Register(Component.For<BrowseDirectoriesViewModel>().ImplementedBy<BrowseDirectoriesViewModel>().LifestyleTransient());
        Container.Register(Component.For<ExportViewModel>().ImplementedBy<ExportViewModel>().LifestyleTransient());
        */
    }

    public static void Reset()
    {
        Container = new WindsorContainer();
        Bootstrap();
    }
}

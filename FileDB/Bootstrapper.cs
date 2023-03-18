using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FileDB.Model;
using FileDB.Notifiers;
using FileDB.ViewModel;

namespace FileDB;

public static class Bootstrapper
{
    public static WindsorContainer Container { get; } = new WindsorContainer();

    public static void Bootstrap()
    {
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
    }
}

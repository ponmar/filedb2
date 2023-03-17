using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace FileDB;

public static class Bootstrapper
{
    public static WindsorContainer Container { get; } = new WindsorContainer();

    public static void Bootstrap()
    {
        Container.Register(Component.For<IDialogs>().ImplementedBy<Dialogs>());
    }
}

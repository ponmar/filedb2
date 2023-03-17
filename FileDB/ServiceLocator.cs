namespace FileDB;

public class ServiceLocator
{
    public static T Resolve<T>()
    {
        return Bootstrapper.Container.Resolve<T>();
    }
}

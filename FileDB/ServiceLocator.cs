using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace FileDB;

public class ServiceLocator
{
    private static WindsorContainer Container { get; } = new();

    public static void RegisterSingleton<T>() where T : class
    {
        RegisterSingleton<T, T>();
    }

    public static void RegisterSingleton<TService, TImplementedBy>() where TService : class where TImplementedBy : TService
    {
        Container.Register(Component.For<TService>().ImplementedBy<TImplementedBy>());
    }

    public static void RegisterSingleton<TImplementedBy>(params Type[] services) where TImplementedBy : class
    {
        foreach (var service in services)
        {
            var implementedByType = typeof(TImplementedBy);
            if (!service.IsAssignableFrom(implementedByType))
            {
                throw new ArgumentException($"Registered type {implementedByType} does not implement {service}");
            }
        }

        Container.Register(Component.For(services).ImplementedBy<TImplementedBy>());
    }

    public static void RegisterSingleton<TService>(TService instance) where TService : class
    {
        Container.Register(Component.For<TService>().Instance(instance));
    }

    public static void RegisterTransient<TServiceAndImplementation>() where TServiceAndImplementation : class
    {
        RegisterTransient<TServiceAndImplementation, TServiceAndImplementation>();
    }

    public static void RegisterTransient<TService, TImplementedBy>() where TService : class where TImplementedBy : TService
    {
        Container.Register(Component.For<TService>().ImplementedBy<TImplementedBy>().LifestyleTransient());
    }

    public static TService Resolve<TService>()
    {
        return Container.Resolve<TService>();
    }

    public static object Resolve(Type serviceType)
    {
        return Container.Resolve(serviceType);
    }

    public static T Resolve<T>(string key, object? value)
    {
        if (value is null)
        {
            return Resolve<T>();
        }

        var a = new Arguments
        {
            { key, value }
        };
        return Container.Resolve<T>(a);
    }
}

using CommunityToolkit.Mvvm.Messaging;
using System;

namespace FileDB;

public static class Events
{
    public static void Register<T>(Action<T> a, object recepient) where T : class
    {
        WeakReferenceMessenger.Default.Register<T>(recepient, (r, m) =>
        {
            a.Invoke(m);
        });
    }

    public static void Send<T>(T e) where T : class
    {
        WeakReferenceMessenger.Default.Send(e);
    }

    public static void Send<T>() where T : class
    {
        var instance = Activator.CreateInstance<T>();
        Send(instance);
    }
}

public static class ObjectExtensions
{
    public static void RegisterForEvent<T>(this object e, Action<T> a) where T : class
    {
        Events.Register(a, e);
    }
}

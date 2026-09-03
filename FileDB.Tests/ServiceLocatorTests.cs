using Castle.MicroKernel;
using FileDB.Infrastructure;
using Xunit;

namespace FileDB.Tests;

public class ServiceLocatorTests : IDisposable
{
    public ServiceLocatorTests()
    {
        ServiceLocator.Reset();
    }

    public void Dispose()
    {
        ServiceLocator.Reset();
    }

    // --- RegisterSingleton<T> ---

    [Fact]
    public void RegisterSingleton_CanResolve()
    {
        ServiceLocator.RegisterSingleton<FakeService>();
        var result = ServiceLocator.Resolve<FakeService>();
        Assert.NotNull(result);
    }

    [Fact]
    public void RegisterSingleton_ReturnsSameInstanceEachTime()
    {
        ServiceLocator.RegisterSingleton<FakeService>();
        var first = ServiceLocator.Resolve<FakeService>();
        var second = ServiceLocator.Resolve<FakeService>();
        Assert.Same(first, second);
    }

    // --- RegisterSingleton<TService, TImplementedBy> ---

    [Fact]
    public void RegisterSingleton_InterfaceAndImpl_CanResolveByInterface()
    {
        ServiceLocator.RegisterSingleton<IFakeService, FakeService>();
        var result = ServiceLocator.Resolve<IFakeService>();
        Assert.NotNull(result);
        Assert.IsType<FakeService>(result);
    }

    [Fact]
    public void RegisterSingleton_InterfaceAndImpl_IsSingleton()
    {
        ServiceLocator.RegisterSingleton<IFakeService, FakeService>();
        var first = ServiceLocator.Resolve<IFakeService>();
        var second = ServiceLocator.Resolve<IFakeService>();
        Assert.Same(first, second);
    }

    // --- RegisterSingleton with multiple service types ---

    [Fact]
    public void RegisterSingleton_MultipleServiceTypes_ResolvableByEach()
    {
        ServiceLocator.RegisterSingleton<FakeMultiService>(
            typeof(IFakeService),
            typeof(IFakeService2));

        var byFirst = ServiceLocator.Resolve<IFakeService>();
        var bySecond = ServiceLocator.Resolve<IFakeService2>();

        Assert.NotNull(byFirst);
        Assert.NotNull(bySecond);
    }

    [Fact]
    public void RegisterSingleton_MultipleServiceTypes_SameInstanceForAll()
    {
        ServiceLocator.RegisterSingleton<FakeMultiService>(
            typeof(IFakeService),
            typeof(IFakeService2));

        var byFirst = ServiceLocator.Resolve<IFakeService>();
        var bySecond = ServiceLocator.Resolve<IFakeService2>();

        Assert.Same(byFirst, bySecond);
    }

    [Fact]
    public void RegisterSingleton_MultipleServiceTypes_NotImplementedType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            ServiceLocator.RegisterSingleton<FakeService>(typeof(IFakeService2)));
    }

    // --- RegisterSingleton<TService>(instance) ---

    [Fact]
    public void RegisterSingleton_Instance_ResolvesToSameInstance()
    {
        var instance = new FakeService();
        ServiceLocator.RegisterSingleton<IFakeService>(instance);
        var resolved = ServiceLocator.Resolve<IFakeService>();
        Assert.Same(instance, resolved);
    }

    // --- RegisterTransient ---

    [Fact]
    public void RegisterTransient_CanResolve()
    {
        ServiceLocator.RegisterTransient<FakeService>();
        var result = ServiceLocator.Resolve<FakeService>();
        Assert.NotNull(result);
    }

    [Fact]
    public void RegisterTransient_ReturnsDifferentInstanceEachTime()
    {
        ServiceLocator.RegisterTransient<FakeService>();
        var first = ServiceLocator.Resolve<FakeService>();
        var second = ServiceLocator.Resolve<FakeService>();
        Assert.NotSame(first, second);
    }

    [Fact]
    public void RegisterTransient_InterfaceAndImpl_CanResolveByInterface()
    {
        ServiceLocator.RegisterTransient<IFakeService, FakeService>();
        var result = ServiceLocator.Resolve<IFakeService>();
        Assert.NotNull(result);
        Assert.IsType<FakeService>(result);
    }

    [Fact]
    public void RegisterTransient_InterfaceAndImpl_ReturnsDifferentInstances()
    {
        ServiceLocator.RegisterTransient<IFakeService, FakeService>();
        var first = ServiceLocator.Resolve<IFakeService>();
        var second = ServiceLocator.Resolve<IFakeService>();
        Assert.NotSame(first, second);
    }

    // --- Resolve(Type) ---

    [Fact]
    public void Resolve_ByType_ReturnsInstance()
    {
        ServiceLocator.RegisterSingleton<FakeService>();
        var result = ServiceLocator.Resolve(typeof(FakeService));
        Assert.NotNull(result);
        Assert.IsType<FakeService>(result);
    }

    // --- Resolve<T>(key, value) ---

    [Fact]
    public void Resolve_WithNullValue_ResolvesNormally()
    {
        ServiceLocator.RegisterSingleton<FakeService>();
        // null value means: ignore the key/value, fall back to standard Resolve<T>()
        var result = ServiceLocator.Resolve<FakeService>("anyKey", null);
        Assert.NotNull(result);
    }

    [Fact]
    public void Resolve_WithNamedArgument_PassesArgumentToConstructor()
    {
        ServiceLocator.RegisterTransient<FakeServiceWithArg>();
        var result = ServiceLocator.Resolve<FakeServiceWithArg>("message", "hello");
        Assert.Equal("hello", result.Message);
    }

    // --- Reset ---

    [Fact]
    public void Reset_ClearsRegistrations()
    {
        ServiceLocator.RegisterSingleton<FakeService>();
        ServiceLocator.Reset();
        Assert.Throws<ComponentNotFoundException>(() => ServiceLocator.Resolve<FakeService>());
    }

    [Fact]
    public void Reset_AllowsReRegistration()
    {
        ServiceLocator.RegisterSingleton<FakeService>();
        ServiceLocator.Reset();
        ServiceLocator.RegisterSingleton<FakeService>(); // must not throw
        Assert.NotNull(ServiceLocator.Resolve<FakeService>());
    }

    // --- Resolve unregistered ---

    [Fact]
    public void Resolve_UnregisteredType_ThrowsComponentNotFoundException()
    {
        Assert.Throws<ComponentNotFoundException>(() => ServiceLocator.Resolve<FakeService>());
    }

    // --- Helper types ---

    private interface IFakeService { }
    private interface IFakeService2 { }
    private class FakeService : IFakeService { }
    private class FakeMultiService : IFakeService, IFakeService2 { }

    private class FakeServiceWithArg
    {
        public string Message { get; }
        public FakeServiceWithArg(string message) => Message = message;
    }
}

using FileDB;
using FileDB.Infrastructure;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using Xunit;

namespace FileDBTests;

/// <summary>
/// Tests for Bootstrapper.Bootstrap().
/// Each test resets and restores the ServiceLocator container so other test
/// classes that use ServiceLocator are not affected.
/// </summary>
public class BootstrapperTests : IDisposable
{
    public BootstrapperTests()
    {
        ServiceLocator.Reset();
    }

    public void Dispose()
    {
        ServiceLocator.Reset();
    }

    [Fact]
    public void Bootstrap_DoesNotThrow()
    {
        Bootstrapper.Bootstrap();
    }

    [Fact]
    public void Bootstrap_AfterReset_CanBeCalledAgainWithoutThrow()
    {
        Bootstrapper.Bootstrap();
        ServiceLocator.Reset();
        Bootstrapper.Bootstrap(); // Must not throw "component already registered"
    }

    [Fact]
    public void Bootstrap_ResolvesIFileSystem()
    {
        Bootstrapper.Bootstrap();
        var service = ServiceLocator.Resolve<IFileSystem>();
        Assert.NotNull(service);
    }

    [Fact]
    public void Bootstrap_ResolvesILoggerFactory()
    {
        Bootstrapper.Bootstrap();
        var service = ServiceLocator.Resolve<ILoggerFactory>();
        Assert.NotNull(service);
    }

    [Fact]
    public void Bootstrap_IFileSystem_IsSingleton()
    {
        Bootstrapper.Bootstrap();
        var first = ServiceLocator.Resolve<IFileSystem>();
        var second = ServiceLocator.Resolve<IFileSystem>();
        Assert.Same(first, second);
    }
}

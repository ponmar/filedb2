using FileDB.Infrastructure;
using Microsoft.Extensions.Logging;
using FakeItEasy;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using Xunit;

namespace FileDB.Tests;

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

    [Fact]
    public void EnsureLogDirectory_CreatesApplicationSpecificLogDirectory()
    {
        var fileSystem = A.Fake<IFileSystem>();
        var expectedLogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Utils.ApplicationName);

        var ensureLogDirectory = typeof(Bootstrapper).GetMethod(
            "EnsureLogDirectory",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(ensureLogDirectory);

        var logDirectory = Assert.IsType<string>(ensureLogDirectory.Invoke(null, [fileSystem]));

        Assert.Equal(expectedLogDirectory, logDirectory);
        A.CallTo(() => fileSystem.Directory.CreateDirectory(expectedLogDirectory))
            .MustHaveHappenedOnceExactly();
    }

}

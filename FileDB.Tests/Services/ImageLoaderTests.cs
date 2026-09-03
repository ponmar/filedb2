using Avalonia.Media.Imaging;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Services;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.CompilerServices;
using Xunit;

namespace FileDB.Tests.Services;

public class ImageLoaderTests
{
    [Fact]
    public void LoadImage_WhenImageIsCached_SendsImageLoaded()
    {
        var fileSystem = new MockFileSystem();
        var recorder = new EventRecorder();
        recorder.Record<ImageLoaded>();

        var filePath = "/cached.png";
        var loader = new ImageLoader(CreateConfigProvider(4), fileSystem);
        loader.ImageCache[filePath] = new ImageLoadResult { Image = CreateBitmapStub() };

        loader.LoadImage(filePath);

        var loaded = recorder.AssertEventRecorded<ImageLoaded>();
        Assert.Equal(filePath, loaded.FilePath);
        Assert.Same(loader.ImageCache[filePath].Image, loaded.Image);
    }

    [Fact]
    public void LoadImage_WhenCacheLimitIsReached_EvictsOlderImages()
    {
        var fileSystem = new MockFileSystem();
        var firstFilePath = "/first.png";
        var secondFilePath = "/second.png";
        var thirdFilePath = "/third.png";

        var loader = new ImageLoader(CreateConfigProvider(2), fileSystem);
        loader.ImageCache[firstFilePath] = new ImageLoadResult { Image = CreateBitmapStub() };
        loader.ImageCache[secondFilePath] = new ImageLoadResult { Image = CreateBitmapStub() };

        loader.LoadImage(thirdFilePath);

        Assert.Equal(2, loader.ImageCache.Count);
        Assert.True(loader.ImageCache.ContainsKey(thirdFilePath));
        Assert.True(!loader.ImageCache.ContainsKey(firstFilePath) || !loader.ImageCache.ContainsKey(secondFilePath));
    }

    [Fact]
    public void SingleImageLoader_Load_WhenImageIsInvalid_SendsImageLoadError()
    {
        var fileSystem = new MockFileSystem();
        var recorder = new EventRecorder();
        recorder.Record<ImageLoadError>();

        var filePath = "/invalid.png";
        fileSystem.AddFile(filePath, new MockFileData("not an image"));

        var loader = new ImageLoader(CreateConfigProvider(4), fileSystem);
        loader.ImageCache[filePath] = new ImageLoadResult();

        new SingleImageLoader(loader, filePath, fileSystem).Load();

        var error = recorder.AssertEventRecorded<ImageLoadError>();
        Assert.Equal(filePath, error.FilePath);
        Assert.NotNull(loader.ImageCache[filePath].Exception);
    }

    private static IConfigProvider CreateConfigProvider(int cacheCount)
    {
        return new FakeConfigProvider(new ConfigBuilder
        {
            ImageMemoryCacheCount = cacheCount,
        }.Build());
    }

    private static Bitmap CreateBitmapStub()
    {
        return (Bitmap)RuntimeHelpers.GetUninitializedObject(typeof(Bitmap));
    }

    private sealed class FakeConfigProvider : IConfigProvider
    {
        public FakeConfigProvider(Config config)
        {
            Config = config;
            FilePaths = new ApplicationFilePaths(Path.GetTempPath(), "config", "database");
        }

        public ApplicationFilePaths FilePaths { get; }
        public Config Config { get; }
    }
}

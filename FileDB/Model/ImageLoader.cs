using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;

namespace FileDB.Model;

public interface IImageLoader
{
    void LoadImage(string filePath);
}

public class ImageLoadResult
{
    public BitmapImage? Image { get; set; } = null;
    public Exception? Exception { get; set; } = null;
}

public class ImageLoader : IImageLoader
{
    private readonly Random random = new();

    public ConcurrentDictionary<string, ImageLoadResult> ImageCache { get; } = new();

    private readonly IConfigProvider configProvider;

    public ImageLoader(IConfigProvider configProvider)
    {
        this.configProvider = configProvider;
    }

    public void LoadImage(string filePath)
    {
        if (ImageCache.TryGetValue(filePath, out var loadResult) && loadResult.Exception is null)
        {
            if (loadResult.Image is not null)
            {
                Events.Send(new ImageLoaded(filePath, loadResult.Image));
            }
            return;
        }

        var loadedImages = ImageCache.Values.Where(x => x.Image is not null).ToList();
        while (loadedImages.Any() && loadedImages.Count >= configProvider.Config.ImageMemoryCacheCount)
        {
            int randomIndex = random.Next(loadedImages.Count - 1);
            var imageToRemove = loadedImages[randomIndex];
            loadedImages.RemoveAt(randomIndex);

            var itemsToRemove = ImageCache.Where(kvp => kvp.Value.Equals(imageToRemove));
            foreach (var itemToRemove in itemsToRemove)
            {
                ImageCache.TryRemove(itemToRemove.Key, out _);
            }
        }

        ImageCache[filePath] = new ImageLoadResult();

        var thread = new Thread(new ThreadStart(new SingleImageLoader(this, filePath).Load));
        thread.Start();
    }
}

public class SingleImageLoader
{
    private readonly ImageLoader imageLoader;
    private readonly string filePath;

    public SingleImageLoader(ImageLoader imageLoader, string filePath)
    {
        this.imageLoader = imageLoader;
        this.filePath = filePath;
    }

    public void Load()
    {
        var loadResult = imageLoader.ImageCache[filePath];

        try
        {
            var uri = new Uri(filePath, UriKind.Absolute);
            loadResult.Image = new BitmapImage(uri);
            loadResult.Image.Freeze();
            Events.Send(new ImageLoaded(filePath, loadResult.Image));
        }
        catch (Exception e)
        {
            loadResult.Exception = e;
            Events.Send(new ImageLoadError(filePath, e));
        }
    }
}
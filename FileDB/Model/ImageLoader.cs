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

public class ImageLoader : IImageLoader
{
    public const int CacheMaxSize = 5;
    private readonly Random random = new();

    public ConcurrentDictionary<string, BitmapImage?> ImageCache { get; } = new();

    public void LoadImage(string filePath)
    {
        if (ImageCache.TryGetValue(filePath, out var image))
        {
            if (image != null)
            {
                Events.Send(new ImageLoaded(filePath, image));
            }
            return;
        }

        if (ImageCache.Keys.Count == CacheMaxSize)
        {
            int randomIndex = random.Next(0, ImageCache.Keys.Count);
            var imageToRemove = ImageCache.ElementAt(randomIndex);
            ImageCache.TryRemove(imageToRemove);
        }

        ImageCache[filePath] = null;

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
        BitmapImage image;
        try
        {
            var uri = new Uri(filePath, UriKind.Absolute);
            image = new BitmapImage(uri);
        }
        catch (Exception e)
        {
            Events.Send(new ImageLoadError(filePath, e));
            return;
        }
        image.Freeze();

        imageLoader.ImageCache[filePath] = image;
        Events.Send(new ImageLoaded(filePath, image));
    }
}
﻿using System;
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

    private readonly IConfigRepository configRepository;

    public ImageLoader(IConfigRepository configRepository)
    {
        this.configRepository = configRepository;
    }

    public void LoadImage(string filePath)
    {
        if (ImageCache.TryGetValue(filePath, out var loadResult) && loadResult.Exception == null)
        {
            if (loadResult.Image != null)
            {
                Events.Send(new ImageLoaded(filePath, loadResult.Image));
            }
            return;
        }

        var loadedImages = ImageCache.Values.Where(x => x.Image != null);
        while (loadedImages.Any() && loadedImages.Count() >= configRepository.Config.ImageMemoryCacheCount)
        {
            int randomIndex = random.Next(0, loadedImages.Count());
            var itemToRemove = ImageCache.ElementAt(randomIndex);
            ImageCache.TryRemove(itemToRemove);
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
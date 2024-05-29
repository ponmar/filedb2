using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;

namespace FileDB;

public interface IClipboardService
{
    Task SetTextAsync(string text);
    Task ClearAsync();
}

public class ClipboardService : IClipboardService
{
    public async Task SetTextAsync(string text)
    {
        await Clipboard.SetTextAsync(text);
    }

    public async Task ClearAsync()
    {
        await Clipboard.ClearAsync();
    }

    private static IClipboard Clipboard
    {
        get
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                var clipboard = desktopApp.MainWindow!.Clipboard!;
                if (clipboard is not null)
                {
                    return clipboard;
                }
            }
            throw new NotSupportedException();
        }
    }
}

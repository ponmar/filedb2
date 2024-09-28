using System;
using System.Collections.Generic;
using System.Linq;
using MsBox.Avalonia;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace MediaFilesHelper2.Dialogs;

public class Dialogs : IDialogs
{
    public const string AppName = "MediaFilesHelper2";

    public async Task ShowInfoDialogAsync(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(AppName, message, ButtonEnum.Ok, Icon.Info);
        var parent = GetParentWindow();
        if (parent is not null)
        {
            await box.ShowWindowDialogAsync(parent);
        }
        else
        {
            await box.ShowWindowAsync();
        }
    }

    public async Task ShowWarningDialogAsync(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(AppName, message, ButtonEnum.Ok, Icon.Warning);
        var parent = GetParentWindow();
        if (parent is not null)
        {
            await box.ShowWindowDialogAsync(parent);
        }
        else
        {
            await box.ShowWindowAsync();
        }
    }

    public async Task ShowErrorDialogAsync(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(AppName, message, ButtonEnum.Ok, Icon.Error);
        var parent = GetParentWindow();
        if (parent is not null)
        {
            await box.ShowWindowDialogAsync(parent);
        }
        else
        {
            await box.ShowWindowAsync();
        }
    }
    
    public async Task ShowErrorDialogAsync(string message, Exception e)
    {
        await ShowErrorDialogAsync($"{message}\n\nException: {e.Message}");
    }

    public async Task ShowErrorDialogAsync(IEnumerable<string> messages)
    {
        await ShowErrorDialogAsync(string.Join("\n", messages));
    }

    public async Task<bool> ShowConfirmDialogAsync(string question)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(AppName, question, ButtonEnum.YesNo, Icon.Question);
        var parent = GetParentWindow();

        if (parent is not null)
        {
            var result = await box.ShowWindowDialogAsync(parent);
            return result == ButtonResult.Yes;
        }
        else
        {
            var result = await box.ShowAsync();
            return result == ButtonResult.Yes;
        }
    }

    public static Window? GetParentWindow()
    {
        return Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp ? desktopApp.MainWindow : null;
    }

    /*
    public void ShowProgressDialog(Action<IProgress<string>> work)
    {
        var parent = GetParentWindow();
        if (parent is not null)
        {
            var splash = new SplashWindow();

            splash.Loaded += (_, args) =>
            {
                var worker = new BackgroundWorker();
                var progress = new Progress<string>(x => splash.Text.Text = x);
                worker.DoWork += (s, workerArgs) => work(progress);
                worker.RunWorkerCompleted += (s, workerArgs) => splash.Close();
                worker.RunWorkerAsync();
            };

            splash.ShowDialog(parent);
        }
    }
    */

    public async Task<string?> ShowBrowseExistingDirectoryDialogAsync(string title)
    {
        return await ShowBrowseExistingDirectoryDialogAsync(title, string.Empty);
    }

    public async Task<string?> ShowBrowseExistingDirectoryDialogAsync(string title, string initialDirectory)
    {
        var parent = GetParentWindow();
        if (parent is null)
        {
            throw new NotSupportedException();
        }

        var topLevel = TopLevel.GetTopLevel(parent);
        if (topLevel is null)
        {
            throw new NotSupportedException();
        }
        var suggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);

        var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            SuggestedStartLocation = suggestedStartLocation,
            AllowMultiple = false,
        });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }
}

using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using MsBox.Avalonia;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;
using FileDB.Views.Dialogs;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FileDB.ViewModels.Search;
using FileDB.ViewModels.Dialogs;
using FileDB.Views.Search.File;
using FileDBInterface.Model;

namespace FileDB.Dialogs;

public class Dialogs : IDialogs
{
    public async Task ShowInfoDialogAsync(string message)
    {
        await ShowMessageBoxAsync(message, ButtonEnum.Ok, Icon.Info);
    }

    public async Task ShowWarningDialogAsync(string message)
    {
        await ShowMessageBoxAsync(message, ButtonEnum.Ok, Icon.Warning);
    }

    public async Task ShowErrorDialogAsync(string message)
    {
        await ShowMessageBoxAsync(message, ButtonEnum.Ok, Icon.Error);
    }
    
    public async Task ShowErrorDialogAsync(string message, Exception e)
    {
        await ShowErrorDialogAsync($"{message}\n\nException: {e.Message}");
    }

    public async Task ShowErrorDialogAsync(IEnumerable<string> messages)
    {
        await ShowErrorDialogAsync(string.Join('\n', messages));
    }

    public async Task ShowErrorDialogAsync(ValidationResult validationResult)
    {
        await ShowErrorDialogAsync(validationResult.Errors.Select(x => x.ErrorMessage));
    }

    public async Task<bool> ShowConfirmDialogAsync(string question)
    {
        var result = await ShowMessageBoxAsync(question, ButtonEnum.YesNo, Icon.Question);
        return result == ButtonResult.Yes;
    }

    public static Window? GetParentWindow()
    {
        return Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp ? desktopApp.MainWindow : null;
    }

    public async Task ShowProgressDialogAsync(Action<IProgress<string>> work)
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

            await splash.ShowDialog(parent);
        }
    }

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

    public async Task<string?> ShowBrowseExistingSubDirectoryDialogAsync(string title, string rootDirectory)
    {
        var selectedDir = await ShowBrowseExistingDirectoryDialogAsync(title, rootDirectory);
        if (selectedDir is null || !selectedDir.StartsWith(rootDirectory, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        var subDir = selectedDir.Substring(rootDirectory.Length).TrimStart(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
        subDir = subDir.Replace(@"\", "/");
        return subDir;
    }

    public async Task<PersonModel?> ShowAddPersonDialogAsync(int? personId = null, string? personName = null)
    {
        var window = new AddPersonWindow(personId, personName);
        await ShowDialog(window);
        return ((AddPersonViewModel)window.DataContext!).AffectedPerson;
    }

    public async Task<LocationModel?> ShowAddLocationDialogAsync(int ?locationId = null, string? locationName = null)
    {
        var window = new AddLocationWindow(locationId, locationName);
        await ShowDialog(window);
        return ((AddLocationViewModel)window.DataContext!).AffectedLocation;
    }

    public async Task<TagModel?> ShowAddTagDialogAsync(int? tagId = null, string? tagName = null)
    {
        var window = new AddTagWindow(tagId, tagName);
        await ShowDialog(window);
        return ((AddTagViewModel)window.DataContext!).AffectedTag;
    }

    public async Task ShowExportSearchResultDialogAsync(SearchResult searchResult)
    {
        var window = new ExportSearchResultWindow();
        var viewModel = (ExportSearchResultViewModel)window.DataContext!;
        viewModel.SearchResult = searchResult;
        await ShowDialog(window);
    }

    public void ShowPresentationWindow(string title)
    {
        var parent = GetParentWindow();
        if (parent is null)
        {
            return;
        }

        var window = new PresentationWindow
        {
            Title = string.Format(title, Utils.ApplicationName, Utils.GetVersionString())
        };
        window.Show(parent);
    }

    private static async Task<ButtonResult> ShowMessageBoxAsync(string message, ButtonEnum buttons, Icon icon)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(Utils.ApplicationName, message, buttons, icon, null, WindowStartupLocation.CenterOwner);
        var parent = GetParentWindow();
        if (parent is not null)
        {
            return await box.ShowWindowDialogAsync(parent);
        }
        else
        {
            return await box.ShowWindowAsync();
        }
    }

    private static async Task ShowDialog(Window window)
    {
        var parent = GetParentWindow();
        if (parent is not null)
        {
            await window.ShowDialog(parent);
        }
    }
}

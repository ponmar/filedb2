using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using MsBox.Avalonia;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;
using FileDBShared.Model;
using FileDBAvalonia.Views.Dialogs;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using FileDBAvalonia.ViewModels;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FileDBAvalonia.ViewModels.Search;
using FileDBAvalonia.ViewModels.Dialogs;

namespace FileDBAvalonia.Dialogs;

public class Dialogs : IDialogs
{
    public async Task ShowInfoDialogAsync(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(Utils.ApplicationName, message, ButtonEnum.Ok, Icon.Info);
        await box.ShowAsync();
    }

    public async Task ShowWarningDialogAsync(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(Utils.ApplicationName, message, ButtonEnum.Ok, Icon.Warning);
        await box.ShowAsync();
    }

    public async Task ShowErrorDialogAsync(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(Utils.ApplicationName, message, ButtonEnum.Ok, Icon.Error);
        await box.ShowAsync();
    }
    
    public async Task ShowErrorDialogAsync(string message, Exception e)
    {
        await ShowErrorDialogAsync($"{message}\n\nException: {e.Message}");
    }

    public async Task ShowErrorDialogAsync(IEnumerable<string> messages)
    {
        await ShowErrorDialogAsync(string.Join("\n", messages));
    }

    public async Task ShowErrorDialogAsync(ValidationResult validationResult)
    {
        await ShowErrorDialogAsync(validationResult.Errors.Select(x => x.ErrorMessage));
    }

    public async Task<bool> ShowConfirmDialogAsync(string question)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(Utils.ApplicationName, question, ButtonEnum.YesNo, Icon.Question);
        var result = await box.ShowAsync();
        return result == ButtonResult.Yes;
    }

    public void ShowProgressDialog(Action<IProgress<string>> work)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp &&
            desktopApp.MainWindow is not null)
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

            splash.ShowDialog(desktopApp.MainWindow);
        }
    }

    public async Task<string?> ShowBrowseExistingDirectoryDialogAsync(string title)
    {
        return await ShowBrowseExistingDirectoryDialogAsync(title, string.Empty);
    }

    public async Task<string?> ShowBrowseExistingDirectoryDialogAsync(string title, string initialDirectory)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktopApp)
        {
            throw new NotSupportedException();
        }

        var topLevel = TopLevel.GetTopLevel(desktopApp.MainWindow);
        var suggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);

        var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            SuggestedStartLocation = suggestedStartLocation,
            AllowMultiple = false,
        });

        return files.Count > 0 ? files[0].Path.AbsolutePath : null;
    }

    public async Task<PersonModel?> ShowAddPersonDialogAsync(int? personId = null)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp &&
            desktopApp.MainWindow is not null)
        {
            var window = new AddPersonWindow(personId);
            await window.ShowDialog(desktopApp.MainWindow);
            return ((AddPersonViewModel)window.DataContext!).AffectedPerson;
        }
        return null;
    }

    public async Task<LocationModel?> ShowAddLocationDialogAsync(int ?locationId = null)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp &&
            desktopApp.MainWindow is not null)
        {
            var window = new AddLocationWindow(locationId);
            await window.ShowDialog(desktopApp.MainWindow);
            return ((AddLocationViewModel)window.DataContext!).AffectedLocation;
        }
        return null;
    }

    public async Task<TagModel?> ShowAddTagDialogAsync(int? tagId = null)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp &&
            desktopApp.MainWindow is not null)
        {
            var window = new AddTagWindow(tagId);
            await window.ShowDialog(desktopApp.MainWindow);
            return ((AddTagViewModel)window.DataContext!).AffectedTag;
        }
        return null;
    }

    public async Task<string?> ShowBrowseDirectoriesDialogAsync()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp &&
            desktopApp.MainWindow is not null)
        {
            var window = new BrowseSubDirectoriesWindow();
            await window.ShowDialog(desktopApp.MainWindow);

            var windowVm = (BrowseSubDirectoriesViewModel)window.DataContext!;
            return windowVm.SelectedDirectoryPath;
        }
        return null;
    }

    public void ShowExportSearchResultDialog(SearchResult searchResult)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp &&
            desktopApp.MainWindow is not null)
        {
            var window = new ExportSearchResultWindow();
            var viewModel = (ExportSearchResultViewModel)window.DataContext!;
            viewModel.SearchResult = searchResult;
            window.ShowDialog(desktopApp.MainWindow);
        }
    }
}

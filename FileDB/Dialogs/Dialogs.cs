﻿using FluentValidation.Results;
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
        var box = MessageBoxManager.GetMessageBoxStandard(Utils.ApplicationName, message, ButtonEnum.Ok, Icon.Info);
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
        var box = MessageBoxManager.GetMessageBoxStandard(Utils.ApplicationName, message, ButtonEnum.Ok, Icon.Warning);
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
        var box = MessageBoxManager.GetMessageBoxStandard(Utils.ApplicationName, message, ButtonEnum.Ok, Icon.Error);
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

    public async Task ShowErrorDialogAsync(ValidationResult validationResult)
    {
        await ShowErrorDialogAsync(validationResult.Errors.Select(x => x.ErrorMessage));
    }

    public async Task<bool> ShowConfirmDialogAsync(string question)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(Utils.ApplicationName, question, ButtonEnum.YesNo, Icon.Question);
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
        var parent = GetParentWindow();
        if (parent is not null)
        {
            var window = new AddPersonWindow(personId, personName);
            await window.ShowDialog(parent);
            return ((AddPersonViewModel)window.DataContext!).AffectedPerson;
        }
        return null;
    }

    public async Task<LocationModel?> ShowAddLocationDialogAsync(int ?locationId = null, string? locationName = null)
    {
        var parent = GetParentWindow();
        if (parent is not null)
        {
            var window = new AddLocationWindow(locationId, locationName);
            await window.ShowDialog(parent);
            return ((AddLocationViewModel)window.DataContext!).AffectedLocation;
        }
        return null;
    }

    public async Task<TagModel?> ShowAddTagDialogAsync(int? tagId = null, string? tagName = null)
    {
        var parent = GetParentWindow();
        if (parent is not null)
        {
            var window = new AddTagWindow(tagId, tagName);
            await window.ShowDialog(parent);
            return ((AddTagViewModel)window.DataContext!).AffectedTag;
        }
        return null;
    }

    public async Task<string?> ShowBrowseDirectoriesDialogAsync()
    {
        var parent = GetParentWindow();
        if (parent is not null)
        {
            var window = new BrowseSubDirectoriesWindow();
            await window.ShowDialog(parent);

            var windowVm = (BrowseSubDirectoriesViewModel)window.DataContext!;
            return windowVm.SelectedDirectoryPath;
        }
        return null;
    }

    public void ShowExportSearchResultDialog(SearchResult searchResult)
    {
        var parent = GetParentWindow();
        if (parent is not null)
        {
            var window = new ExportSearchResultWindow();
            var viewModel = (ExportSearchResultViewModel)window.DataContext!;
            viewModel.SearchResult = searchResult;
            window.ShowDialog(parent);
        }
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
}

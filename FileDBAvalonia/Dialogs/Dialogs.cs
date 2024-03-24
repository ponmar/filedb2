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

    /*
    public string? BrowseExistingFileDialog(string initialDirectory, string filter)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog()
        {
            Filter = filter,
            InitialDirectory = initialDirectory,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? BrowseExistingDirectory(string initialDirectory, string title)
    {
        if (!initialDirectory.EndsWith(Path.DirectorySeparatorChar))
        {
            initialDirectory += Path.DirectorySeparatorChar;
        }

        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = title,
            UseDescriptionForTitle = true,
            SelectedPath = initialDirectory,
            ShowNewFolderButton = true
        };

        return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;
    }

    public string? SelectNewFileDialog(string title, string fileExtension, string filter)
    {
        var dialog = new System.Windows.Forms.OpenFileDialog()
        {
            Title = title,
            DefaultExt = fileExtension,
            Filter = filter,
            CheckFileExists = false,
            CheckPathExists = true,
        };

        var result = dialog.ShowDialog();
        if (result == System.Windows.Forms.DialogResult.OK)
        {
            if (dialog.FileName.EndsWith(fileExtension))
            {
                return dialog.FileName;
            }
            else
            {
                ShowErrorDialog($"File extension must be {fileExtension}");
            }
        }

        return null;
    }
    */

    public PersonModel? ShowAddPersonDialog(int? personId = null)
    {
        /*
        var window = new AddPersonWindow(personId)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();

        return ((AddPersonViewModel)window.DataContext).AffectedPerson;
        */
        return null;
    }

    public LocationModel? ShowAddLocationDialog(int ?locationId = null)
    {
        /*
        var window = new AddLocationWindow(locationId)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();

        return ((AddLocationViewModel)window.DataContext).AffectedLocation;
        */
        return null;
    }

    public TagModel? ShowAddTagDialog(int? tagId = null)
    {
        /*
        var window = new AddTagWindow(tagId)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();

        return ((AddTagViewModel)window.DataContext).AffectedTag;
        */
        return null;
    }

    /*
    public string? ShowBrowseDirectoriesDialog()
    {
        var window = new BrowseDirectoriesWindow
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();

        var windowVm = (BrowseDirectoriesViewModel)window.DataContext;
        return windowVm.SelectedDirectoryPath;
    }

    public void ShowExportDialog(SearchResult searchResult)
    {
        var window = new ExportWindow
        {
            Owner = Application.Current.MainWindow
        };
        var viewModel = (ExportViewModel)window.DataContext;
        viewModel.SearchResult = searchResult;
        window.ShowDialog();
    }
    */
}

using FileDB.View;
using FileDB.ViewModel;
using FileDBShared.Model;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace FileDB;

public interface IDialogs
{
    void ShowInfoDialog(string message);
    void ShowWarningDialog(string message);
    void ShowErrorDialog(string message);
    void ShowErrorDialog(IEnumerable<string> messages);
    void ShowErrorDialog(ValidationResult validationResult);
    bool ShowConfirmDialog(string question);
    void ShowProgressDialog(Action<IProgress<string>> work);
    string? BrowseExistingFileDialog(string initialDirectory, string filter);
    string? BrowseExistingDirectory(string initialDirectory, string title);
    string? SelectNewFileDialog(string title, string fileExtension, string filter);
    PersonModel? ShowAddPersonDialog(int? personId = null);
    LocationModel? ShowAddLocationDialog(int? locationId = null);
    TagModel? ShowAddTagDialog(int? tagId = null);
    string? ShowBrowseDirectoriesDialog();
    void ShowExportDialog(SearchResult searchResult);
    string? ShowCreateDatabaseDialog();
}

public class Dialogs : IDialogs
{
    //public static IDialogs Instance { get; set; } = new Dialogs();

    public void ShowInfoDialog(string message)
    {
        MessageBox.Show(message, Utils.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.No);
    }

    public void ShowWarningDialog(string message)
    {
        MessageBox.Show(message, Utils.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.No);
    }

    public void ShowErrorDialog(string message)
    {
        MessageBox.Show(message, Utils.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.No);
    }

    public void ShowErrorDialog(IEnumerable<string> messages)
    {
        ShowErrorDialog(string.Join("\n", messages));
    }

    public void ShowErrorDialog(ValidationResult validationResult)
    {
        ShowErrorDialog(validationResult.Errors.Select(x => x.ErrorMessage));
    }

    public bool ShowConfirmDialog(string question)
    {
        return MessageBox.Show(question, Utils.ApplicationName, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public void ShowProgressDialog(Action<IProgress<string>> work)
    {
        var splash = new SplashWindow
        {
            Owner = Application.Current.MainWindow
        };

        splash.Loaded += (_, args) =>
        {
            var worker = new BackgroundWorker();
            var progress = new Progress<string>(x => splash.Text.Text = x);
            worker.DoWork += (s, workerArgs) => work(progress);
            worker.RunWorkerCompleted += (s, workerArgs) => splash.Close();
            worker.RunWorkerAsync();
        };

        splash.ShowDialog();
    }

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

    public PersonModel? ShowAddPersonDialog(int? personId = null)
    {
        var window = new AddPersonWindow(personId)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();

        return ((AddPersonViewModel)window.DataContext).AffectedPerson;
    }

    public LocationModel? ShowAddLocationDialog(int ?locationId = null)
    {
        var window = new AddLocationWindow(locationId)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();

        return ((AddLocationViewModel)window.DataContext).AffectedLocation;
    }

    public TagModel? ShowAddTagDialog(int? tagId = null)
    {
        var window = new AddTagWindow(tagId)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();

        return ((AddTagViewModel)window.DataContext).AffectedTag;
    }

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

    public string? ShowCreateDatabaseDialog()
    {
        var window = new CreateDatabaseWindow
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();

        var viewModel = (CreateDatabaseViewModel)window.DataContext;
        return string.IsNullOrEmpty(viewModel.CreatedDatabasePath) ? null : viewModel.CreatedDatabasePath;
    }
}

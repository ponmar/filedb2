using FileDB.View;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace FileDB;

public static class Dialogs
{
    public static void ShowInfoDialog(string message)
    {
        MessageBox.Show(message, Utils.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.No);
    }

    public static void ShowWarningDialog(string message)
    {
        MessageBox.Show(message, Utils.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.No);
    }

    public static void ShowErrorDialog(string message)
    {
        MessageBox.Show(message, Utils.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.No);
    }

    public static void ShowErrorDialog(IEnumerable<string> messages)
    {
        ShowErrorDialog(string.Join("\n", messages));
    }

    public static void ShowErrorDialog(ValidationResult validationResult)
    {
        ShowErrorDialog(validationResult.Errors.Select(x => x.ErrorMessage));
    }

    public static bool ShowConfirmDialog(string question)
    {
        return MessageBox.Show(question, Utils.ApplicationName, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public static void ShowProgressDialog(Action<IProgress<string>> work)
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

    public static string? BrowseExistingFileDialog(string initialDirectory, string filter)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog()
        {
            Filter = filter,
            InitialDirectory = initialDirectory,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public static string? BrowseExistingDirectory(string initialDirectory, string filter)
    {
        var fileDialog = new Microsoft.Win32.OpenFileDialog()
        {
            Filter = filter,
            InitialDirectory = initialDirectory,
            ValidateNames = false,
            CheckFileExists = false,
        };
        if (fileDialog.ShowDialog() == true)
        {
            var dir = Path.GetDirectoryName(fileDialog.FileName);
            if (Directory.Exists(dir))
            {
                return dir;
            }
            else
            {
                Dialogs.ShowErrorDialog($"No such directory: {dir}");
            }
        }
        return null;
    }

    public static string? SelectNewFileDialog(string title, string fileExtension, string filter)
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
}

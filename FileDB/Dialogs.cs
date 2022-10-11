using FileDB.View;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
}

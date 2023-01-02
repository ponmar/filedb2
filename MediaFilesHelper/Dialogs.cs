using System.IO;
using System.Windows;

namespace MediaFilesHelper;

public class Dialogs
{
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

    public void ShowInfoDialog(string caption, string message)
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.No);
    }

    public void ShowWarningDialog(string caption, string message)
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.No);
    }

    public void ShowErrorDialog(string caption, string message)
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.No);
    }

    public bool ShowConfirmDialog(string caption, string question)
    {
        return MessageBox.Show(question, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

}

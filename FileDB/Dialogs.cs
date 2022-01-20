using FluentValidation.Results;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FileDB
{
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
    }
}

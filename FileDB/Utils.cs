using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using FileDB.Configuration;
using FileDB.Extensions;
using FileDB.Sorters;
using FileDBInterface.Model;
using FluentValidation.Results;

namespace FileDB
{
    public class WindowModeDescription
    {
        public string Name => Mode.GetDescription();

        public WindowMode Mode { get; }

        public WindowModeDescription(WindowMode mode)
        {
            Mode = mode;
        }
    }

    public static class Utils
    {
        public const string ApplicationName = "FileDB";

        public static void ShowInfoDialog(string message)
        {
            MessageBox.Show(message, ApplicationName, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.No);
        }

        public static void ShowWarningDialog(string message)
        {
            MessageBox.Show(message, ApplicationName, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.No);
        }

        public static void ShowErrorDialog(string message)
        {
            MessageBox.Show(message, ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.No);
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
            return MessageBox.Show(question, ApplicationName, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public static void OpenUriInBrowser(string uri)
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }

        public static string CreateFileList(List<FilesModel> files)
        {
            return files.Count > 0 ? string.Join(";", files.Select(x => x.Id)) : string.Empty;
        }

        public static List<int> CreateFileIds(string fileList)
        {
            List<int> fileIds = new();

            if (!string.IsNullOrEmpty(fileList))
            {
                var items = fileList.Split(';');
                foreach (var item in items)
                {
                    if (int.TryParse(item, out var fileId))
                    {
                        fileIds.Add(fileId);
                    }
                }
            }

            return fileIds;
        }

        public static List<SortMethodDescription> GetSortMethods()
        {
            List<SortMethodDescription> sortMethods = new();
            foreach (var value in Enum.GetValues<SortMethod>())
            {
                sortMethods.Add(new SortMethodDescription(value));
            }
            return sortMethods;
        }

        public static List<WindowModeDescription> GetWindowModes()
        {
            List<WindowModeDescription> modes = new();
            foreach (var mode in Enum.GetValues<WindowMode>())
            {
                modes.Add(new WindowModeDescription(mode));
            }
            return modes;
        }
    }
}

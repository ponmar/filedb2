﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using FileDB.Config;
using FileDBInterface;
using FileDBInterface.Exceptions;

namespace FileDB
{
    public static class Utils
    {
        public const string ApplicationName = "FileDB";

        public static Config.Config Config { get; set; }

        public static IDatabaseWrapper FileDBHandle
        {
            get
            {
                if (fileDBHandle == null)
                {
                    ReloadFileDBHandle();
                }
                return fileDBHandle;
            }
        }
        private static IDatabaseWrapper fileDBHandle;

        public static void ReloadFileDBHandle()
        {
            try
            {
                fileDBHandle = new DatabaseWrapper(Config.Database, Config.FilesRootDirectory);
            }
            catch (DatabaseWrapperException)
            {
                fileDBHandle = new InvalidHandle();
            }
        }

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

        public static bool ShowConfirmDialog(string question)
        {
            return MessageBox.Show(question, ApplicationName, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public static void OpenUriInBrowser(string uri)
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }
    }
}

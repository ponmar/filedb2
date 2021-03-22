﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileDB2Browser.Config;
using FileDB2Browser.ViewModel;
using FileDB2Interface;

namespace FileDB2Browser
{
    enum Page { Start, Find, Birthdays, Persons, Locations, Tags, Import, Tools }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Page currentPage;
        private readonly FileDB2Handle fileDB2Handle;

        public MainWindow()
        {
            InitializeComponent();

            if (!FileDB2BrowserConfigIO.FileExists())
            {
                var defaultConfig = new FileDB2BrowserConfig()
                {
                    Database = "filedb2.db",
                    FilesRootDirectory = "files",
                };

                if (!FileDB2BrowserConfigIO.Write(defaultConfig))
                {
                    // TODO: show error dialog and exit
                }
            }

            FileDB2BrowserConfig browserConfig = FileDB2BrowserConfigIO.Read();

            var config = new FileDB2Config()
            {
                Database = browserConfig.Database,
                FilesRootDirectory = browserConfig.FilesRootDirectory,
                BlacklistedFilePathPatterns = new List<string>() { "Thumbs.db", "filedb.db", "unsorted", "TN_" },
                WhitelistedFilePathPatterns = new List<string>() { ".jpg", ".png", ".bmp", ".gif", ".avi", ".mpg", ".mp4", ".mkv", ".mov", ".pdf" },
                IncludeHiddenDirectories = false,
            };

            fileDB2Handle = new FileDB2Handle(config);
            SetPage(Page.Start, true);
        }

        private void SetStartPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Start);
        }

        private void SetFindPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Find);
        }

        private void SetBirthdaysPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Birthdays);
        }

        private void SetPersonsPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Persons);
        }

        private void SetLocationsPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Locations);
        }

        private void SetTagsPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Tags);
        }

        private void SetImportPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Import);
        }

        private void SetToolsPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Tools);
        }

        private void SetPage(Page page, bool force = false)
        {
            if (page != currentPage || force)
            {
                currentPage = page;
                StartPage.Visibility = Visibility.Collapsed;
                FindPage.Visibility = Visibility.Collapsed;
                BirthdaysPage.Visibility = Visibility.Collapsed;
                PersonsPage.Visibility = Visibility.Collapsed;
                LocationsPage.Visibility = Visibility.Collapsed;
                TagsPage.Visibility = Visibility.Collapsed;
                ImportPage.Visibility = Visibility.Collapsed;
                ToolsPage.Visibility = Visibility.Collapsed;

                switch (page)
                {
                    case Page.Start:
                        DataContext = new StartViewModel(fileDB2Handle);
                        StartPage.Visibility = Visibility.Visible;
                        break;
                    case Page.Find:
                        DataContext = new FindViewModel(fileDB2Handle);
                        FindPage.Visibility = Visibility.Visible;
                        break;
                    case Page.Birthdays:
                        DataContext = new BirthdaysViewModel(fileDB2Handle);
                        BirthdaysPage.Visibility = Visibility.Visible;
                        break;
                    case Page.Persons:
                        DataContext = new PersonsViewModel(fileDB2Handle);
                        PersonsPage.Visibility = Visibility.Visible;
                        break;
                    case Page.Locations:
                        DataContext = new LocationsViewModel(fileDB2Handle);
                        LocationsPage.Visibility = Visibility.Visible;
                        break;
                    case Page.Tags:
                        DataContext = new TagsViewModel(fileDB2Handle);
                        TagsPage.Visibility = Visibility.Visible;
                        break;
                    case Page.Import:
                        DataContext = new ImportViewModel(fileDB2Handle);
                        ImportPage.Visibility = Visibility.Visible;
                        break;
                    case Page.Tools:
                        DataContext = new ToolsViewModel(fileDB2Handle);
                        ToolsPage.Visibility = Visibility.Visible;
                        break;
                }
            }
        }
    }
}

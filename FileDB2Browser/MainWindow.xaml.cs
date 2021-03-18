using System;
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
using FileDB2Browser.ViewModel;
using FileDB2Interface;

namespace FileDB2Browser
{
    enum Page { Start, Files, Birthdays, Persons, Locations, Tags, Import }

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
            SetPage(Page.Start, true);

            var config = new FileDB2Config()
            {
                Database = @"C:\Source\filedb2_db\test.db",
                FilesRootDirectory = @"X:",
            };

            fileDB2Handle = new FileDB2Handle(config);
        }

        private void SetStartPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Start);
        }

        private void SetFilesPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Files);
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

        private void SetPage(Page page, bool force = false)
        {
            if (page != currentPage || force)
            {
                currentPage = page;
                StartPage.Visibility = Visibility.Collapsed;
                FilesPage.Visibility = Visibility.Collapsed;
                BirthdaysPage.Visibility = Visibility.Collapsed;
                PersonsPage.Visibility = Visibility.Collapsed;
                LocationsPage.Visibility = Visibility.Collapsed;
                TagsPage.Visibility = Visibility.Collapsed;
                ImportPage.Visibility = Visibility.Collapsed;

                switch (page)
                {
                    case Page.Start:
                        DataContext = new StartViewModel(fileDB2Handle);
                        StartPage.Visibility = Visibility.Visible;
                        break;
                    case Page.Files:
                        DataContext = new FilesViewModel(fileDB2Handle);
                        FilesPage.Visibility = Visibility.Visible;
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
                }
            }
        }
    }
}

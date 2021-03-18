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
using FileDB2Browser.ModelView;

namespace FileDB2Browser
{
    enum Page { Start, Files, Birthdays, Categories, Collection }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Page currentPage = Page.Start;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new StartModelView();
        }

        private void Button_Click_StartPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Start);
        }

        private void Button_Click_FilesPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Files);
        }

        private void Button_Click_BirthdaysPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Birthdays);
        }

        private void Button_Click_CategoriesPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Categories);
        }

        private void Button_Click_CollectionPage(object sender, RoutedEventArgs e)
        {
            SetPage(Page.Collection);
        }

        private void SetPage(Page page)
        {
            if (page == currentPage)
            {
                return;
            }

            currentPage = page;
            PageStart.Visibility = Visibility.Collapsed;
            PageFiles.Visibility = Visibility.Collapsed;
            PageBirthdays.Visibility = Visibility.Collapsed;
            PageCategories.Visibility = Visibility.Collapsed;
            PageCollection.Visibility = Visibility.Collapsed;

            switch (page)
            {
                case Page.Start:
                    DataContext = new StartModelView();
                    PageStart.Visibility = Visibility.Visible;
                    break;
                case Page.Files:
                    DataContext = new FilesModelView();
                    PageFiles.Visibility = Visibility.Visible;
                    break;
                case Page.Birthdays:
                    DataContext = new BirthdaysModelView();
                    PageBirthdays.Visibility = Visibility.Visible;
                    break;
                case Page.Categories:
                    DataContext = new CategoriesModelView();
                    PageCategories.Visibility = Visibility.Visible;
                    break;
                case Page.Collection:
                    DataContext = new CollectionModelView();
                    PageCollection.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}

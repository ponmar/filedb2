using System.Windows;
using FileDB.Model;
using FileDB.ViewModel;

namespace FileDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var configRepository = ServiceLocator.Resolve<IConfigRepository>();
            DataContext = new MainViewModel(configRepository, ServiceLocator.Resolve<INotificationHandling>());
        }

        private void TabItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

using System.Windows;
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
            var model = Model.Model.Instance;
            DataContext = new MainViewModel(model.Config, model);
        }

        private void TabItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}

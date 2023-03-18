using System.Windows.Controls;
using FileDB.Model;
using FileDB.ViewModel;

namespace FileDB.View
{
    /// <summary>
    /// Interaction logic for StartView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<SettingsViewModel>();
        }
    }
}

using System.Windows.Controls;
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
            var model = Model.Model.Instance;
            DataContext = new SettingsViewModel(model.Config, model, ServiceLocator.Resolve<IDialogs>());
        }
    }
}

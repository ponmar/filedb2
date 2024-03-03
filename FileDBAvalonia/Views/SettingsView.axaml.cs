using Avalonia.Controls;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<SettingsViewModel>();
    }
}

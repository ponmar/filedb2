using Avalonia.Controls;
using FileDB.Infrastructure;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<SettingsViewModel>();
        }
    }
}

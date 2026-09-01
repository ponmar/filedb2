using Avalonia.Controls;
using FileDB.Infrastructure;
using FileDB.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace FileDB.Views;

[ExcludeFromCodeCoverage]
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

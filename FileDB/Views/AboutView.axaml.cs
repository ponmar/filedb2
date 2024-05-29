using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<AboutViewModel>();
        }
    }
}

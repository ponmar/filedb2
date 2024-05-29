using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<MainViewModel>();
        }
    }
}

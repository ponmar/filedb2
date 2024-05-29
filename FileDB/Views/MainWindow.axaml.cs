using Avalonia;
using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<MainViewModel>();
        }

#if DEBUG
        // Open the developer tools window with F12
        this.AttachDevTools();
#endif
    }
}

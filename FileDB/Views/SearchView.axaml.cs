using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class SearchView : UserControl
{
    public SearchView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<MainViewModel>();
        }
    }
}

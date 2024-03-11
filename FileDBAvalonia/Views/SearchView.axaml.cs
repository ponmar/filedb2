using Avalonia.Controls;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views;

public partial class SearchView : UserControl
{
    public SearchView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<SearchViewModel>();
    }
}

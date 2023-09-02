using System.Windows.Controls;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for SearchView.xaml
/// </summary>
public partial class SearchView : UserControl
{
    public SearchView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<SearchViewModel>();
    }
}

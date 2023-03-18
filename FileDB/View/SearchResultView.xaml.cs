using FileDB.ViewModel;
using System.Windows.Controls;

namespace FileDB.View;

/// <summary>
/// Interaction logic for SearchResultView.xaml
/// </summary>
public partial class SearchResultView : UserControl
{
    public SearchResultView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<FindViewModel>();
    }
}

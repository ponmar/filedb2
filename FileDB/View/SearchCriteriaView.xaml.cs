using FileDB.ViewModel;
using System.Windows.Controls;

namespace FileDB.View;

/// <summary>
/// Interaction logic for SearchCriteriaView.xaml
/// </summary>
public partial class SearchCriteriaView : UserControl
{
    public SearchCriteriaView()
    {
        InitializeComponent();
        DataContext = FindViewModel.Instance;
    }
}

using FileDB.ViewModel;
using System.Windows.Controls;

namespace FileDB.View;

/// <summary>
/// Interaction logic for SearchCriteria.xaml
/// </summary>
public partial class SearchCriteria : UserControl
{
    public SearchCriteria()
    {
        InitializeComponent();
        DataContext = FindViewModel.Instance;
    }
}

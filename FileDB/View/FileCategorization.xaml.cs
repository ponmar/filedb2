using FileDB.ViewModel;
using System.Windows.Controls;

namespace FileDB.View;

/// <summary>
/// Interaction logic for FileCategorization.xaml
/// </summary>
public partial class FileCategorization : UserControl
{
    public FileCategorization()
    {
        InitializeComponent();
        DataContext = FindViewModel.Instance;
    }
}

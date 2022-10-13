using System.Windows.Controls;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for ToolsView.xaml
/// </summary>
public partial class ToolsView : UserControl
{
    public ToolsView()
    {
        InitializeComponent();
        DataContext = new ToolsViewModel();
    }
}

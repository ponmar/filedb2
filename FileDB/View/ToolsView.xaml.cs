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
        var model = Model.Model.Instance;
        DataContext = new ToolsViewModel(model.Config, model.DbAccess, model.FilesystemAccess, model.Dialogs);
    }
}

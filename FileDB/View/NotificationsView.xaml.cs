using System.Windows.Controls;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for ToolsView.xaml
/// </summary>
public partial class NotificationsView : UserControl
{
    public NotificationsView()
    {
        InitializeComponent();
        var model = Model.Model.Instance;
        DataContext = new NotificationsViewModel(model.Config, model.DbAccess, model.NotifierFactory, model);
    }
}

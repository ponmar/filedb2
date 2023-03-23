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
        DataContext = ServiceLocator.Resolve<NotificationsViewModel>();
    }
}

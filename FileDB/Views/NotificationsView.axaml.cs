using Avalonia.Controls;
using FileDB.Infrastructure;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class NotificationsView : UserControl
{
    public NotificationsView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<NotificationsViewModel>();
        }
    }
}

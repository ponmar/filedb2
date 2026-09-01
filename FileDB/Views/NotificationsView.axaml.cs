using Avalonia.Controls;
using FileDB.Infrastructure;
using FileDB.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace FileDB.Views;

[ExcludeFromCodeCoverage]
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

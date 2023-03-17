using System.Windows.Controls;
using System.Windows.Input;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for LocationsView.xaml
/// </summary>
public partial class LocationsView : UserControl
{
    public LocationsView()
    {
        InitializeComponent();
        var model = Model.Model.Instance;
        DataContext = new LocationsViewModel(model.Config, model.DbAccess, ServiceLocator.Resolve<IDialogs>());
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Handle event here to avoid no scrolling over DataGrid within this ScrollViewer
        ScrollViewer scv = (ScrollViewer)sender;
        scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
        e.Handled = true;
    }
}

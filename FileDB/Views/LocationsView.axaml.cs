using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class LocationsView : UserControl
{
    public LocationsView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<LocationsViewModel>();
        }
    }
}

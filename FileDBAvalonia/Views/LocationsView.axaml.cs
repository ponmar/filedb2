using Avalonia.Controls;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views
{
    public partial class LocationsView : UserControl
    {
        public LocationsView()
        {
            InitializeComponent();
            DataContext = ServiceLocator.Resolve<LocationsViewModel>();
        }
    }
}

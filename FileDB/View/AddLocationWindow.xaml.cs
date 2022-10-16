using System.Windows;
using FileDB.ViewModel;

namespace FileDB.View;

/// <summary>
/// Interaction logic for AddLocationWindow.xaml
/// </summary>
public partial class AddLocationWindow : Window
{
    private readonly Model.Model model = Model.Model.Instance;

    public AddLocationWindow(int? locationId = null)
    {
        InitializeComponent();
        DataContext = new AddLocationViewModel(locationId);

        model.LocationsUpdated += Model_LocationsUpdated;
    }

    private void Model_LocationsUpdated(object? sender, System.EventArgs e)
    {
        Close();
    }
}

using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class UpdateLocationsView : UserControl
{
    public UpdateLocationsView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<UpdateLocationsViewModel>();
        }
    }
}

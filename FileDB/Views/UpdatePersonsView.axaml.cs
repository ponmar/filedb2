using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class UpdatePersonsView : UserControl
{
    public UpdatePersonsView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<UpdatePersonsViewModel>();
        }
    }
}

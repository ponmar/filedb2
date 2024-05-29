using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class PersonsView : UserControl
{
    public PersonsView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<PersonsViewModel>();
        }
    }
}

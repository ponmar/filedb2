using Avalonia.Controls;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views;

public partial class PersonsView : UserControl
{
    public PersonsView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<PersonsViewModel>();
    }
}

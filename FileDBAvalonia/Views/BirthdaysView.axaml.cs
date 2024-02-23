using Avalonia.Controls;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views;

public partial class BirthdaysView : UserControl
{
    public BirthdaysView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<BirthdaysViewModel>();
    }
}

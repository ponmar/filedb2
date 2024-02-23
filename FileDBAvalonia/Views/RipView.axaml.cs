using Avalonia.Controls;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Views;

public partial class RipView : UserControl
{
    public RipView()
    {
        InitializeComponent();
        DataContext = ServiceLocator.Resolve<RipViewModel>();
    }
}

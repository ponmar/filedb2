using Avalonia.Controls;
using FileDB.Infrastructure;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class RipView : UserControl
{
    public RipView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<RipViewModel>();
        }
    }
}

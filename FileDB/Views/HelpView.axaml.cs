using Avalonia.Controls;
using FileDB.ViewModels;

namespace FileDB.Views;

public partial class HelpView : UserControl
{
    public HelpView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            DataContext = ServiceLocator.Resolve<HelpViewModel>();
        }
    }
}
